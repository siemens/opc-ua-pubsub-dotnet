// Copyright 2020 Siemens AG
// SPDX-License-Identifier: MIT

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using opc.ua.pubsub.dotnet.binary.DataPoints;
using opc.ua.pubsub.dotnet.binary.Decode;
using opc.ua.pubsub.dotnet.binary.Header;
using opc.ua.pubsub.dotnet.binary.Messages;
using opc.ua.pubsub.dotnet.binary.Messages.Chunk;
using opc.ua.pubsub.dotnet.binary.Messages.Delta;
using opc.ua.pubsub.dotnet.binary.Messages.Key;
using opc.ua.pubsub.dotnet.binary.Messages.Meta;
using opc.ua.pubsub.dotnet.binary.Messages.Meta.Structure;
using opc.ua.pubsub.dotnet.client;
using opc.ua.pubsub.dotnet.client.common;
using opc.ua.pubsub.dotnet.client.common.Settings;
using opc.ua.pubsub.dotnet.client.Interfaces;
using opc.ua.pubsub.dotnet.visualizer.OPC;
using opc.ua.pubsub.dotnet.visualizer.UI;
using Client = opc.ua.pubsub.dotnet.client.Client;
using String = opc.ua.pubsub.dotnet.binary.String;

namespace opc.ua.pubsub.dotnet.visualizer
{
    public partial class VisualizerForm : Form
    {
        private const      int CustomColumnIndex = 5;
        private const      int MDSPColumnIndex = 6;
        private const      int OrcatColumnIndex = 3;
        private const      int OriginalTimeStamp = 8;
        private const      int QualityColumnIndex = 4;
        private const      int TimestampIndex = 7;
        private readonly   Dictionary<string, Dictionary<ushort, List<int>>> m_GroupDataIndex = new Dictionary<string, Dictionary<ushort, List<int>>>();
        private readonly   ConcurrentDictionary<string, ConcurrentDictionary<ushort, BindingSource>> m_PublisherBindings;
        protected readonly Settings m_Settings;
        private readonly   UpdatePublisher m_UpdatePublisher;
        private readonly   UpdateValues m_UpdateValues;
        private            string m_CurrentPublisherId;
        private            ushort m_CurrentWriterId;
        private            BindingSource m_PublisherBindingSource;

        public VisualizerForm( Settings settings )
        {
            m_Settings = settings;
            InitializeComponent();
            InitBinding();
            InitTypeConverters();
            IClientService client = new Client( m_Settings );
            client.Options.LegacyFieldFlagEncoding =  false;
            client.MessageReceived                 += ClientOnMessageReceived;
            client.MetaMessageReceived             += ClientOnMessageReceived;
            BlockingCollection<Publisher> publisherQueue = new BlockingCollection<Publisher>();
            m_PublisherBindings = new ConcurrentDictionary<string, ConcurrentDictionary<ushort, BindingSource>>();
            BlockingCollection<DataPointCollection> valueQueue = new BlockingCollection<DataPointCollection>();
            Parser                                  parser     = new Parser( publisherQueue, valueQueue, this, m_PublisherBindings );
            client.MessageReceived     += parser.OnMessageDecoded;
            client.MetaMessageReceived += parser.OnMessageDecoded;
            client.FileReceived        += parser.ClientOnFileReceived;
            m_UpdatePublisher          =  new UpdatePublisher( publisherQueue, m_PublisherBindingSource, this );
            m_UpdateValues             =  new UpdateValues( valueQueue, m_PublisherBindings, this );

            // there was a decoding standard violation in a part of the meta frame
            // which has been fixed and we enable it here to use this fix
            client.Options.LegacyFieldFlagEncoding = false;
            if ( m_Settings.Client.UseTls )
            {
                // with TLS for use e.g. with MQTT-Broker as MDSP simulation

                // Broker CA certificate
                if ( SettingManager.TryGetCertificateAsArray( m_Settings.Client.BrokerCACert, out byte[] brokerCaCert ) )
                {
                    client.BrokerCACert = brokerCaCert;
                }

                // client certificate and client CA 
                MindsphereClientCredentials credentials;

                // Client credentials
                credentials = new MindsphereClientCredentials();
                if ( SettingManager.TryGetCertificateAsArray( m_Settings.Client.ClientCertP12, out byte[] clientPkcs12Content ) )
                {
                    credentials.Import( clientPkcs12Content, m_Settings.Client.ClientCertPassword );
                }
                client.Connect( credentials );
            }
            else
            {
                // without TLS for use e.g. with internal broker of GridEdge
                client.Connect();
            }
            client.Subscribe();
            Task.Run( () => m_UpdatePublisher.Start() );
            Task.Run( () => m_UpdateValues.Start() );
        }

        public void AddGroupDataTypeIndex( string publisherId, ushort writerId, int index )
        {
            if ( m_GroupDataIndex.ContainsKey( publisherId ) == false )
            {
                Dictionary<ushort, List<int>> temp = new Dictionary<ushort, List<int>>();
                temp.Add( writerId, new List<int>() );
                m_GroupDataIndex.Add( publisherId, temp );
            }
            if ( m_GroupDataIndex[publisherId]
                        .ContainsKey( writerId )
              == false )
            {
                Dictionary<ushort, List<int>> temp = new Dictionary<ushort, List<int>>();
                temp.Add( writerId, new List<int>() );
                m_GroupDataIndex.Add( publisherId, temp );
            }
            m_GroupDataIndex[publisherId][writerId]
                   .Add( index );
        }

        public void AddNewPublisher( Publisher publisher )
        {
            if ( publisher == null )
            {
                return;
            }
            TreeNode parent = null;
            if ( !DevicesTreeView.Nodes.ContainsKey( publisher.PublisherID ) )
            {
                parent = new TreeNode( publisher.PublisherID )
                         {
                                 Name = publisher.PublisherID
                         };
                DevicesTreeView.Nodes.Add( parent );
            }
            else
            {
                parent = DevicesTreeView.Nodes[publisher.PublisherID];
            }
            string key = publisher.DataSetWriterID.ToString();
            if ( parent.Nodes.ContainsKey( key ) )
            {
                return;
            }
            TreeNode node = new TreeNode( key )
                            {
                                    Name = key,
                                    Tag  = publisher.DataSetWriterID
                            };
            parent.Nodes.Add( node );
        }

        public void ResetGroupedTypeIndex( string publisherId, ushort writerId )
        {
            if ( m_GroupDataIndex.ContainsKey( publisherId )
              && m_GroupDataIndex[publisherId]
                        .ContainsKey( writerId ) )
            {
                List<int> groupedDataTypeIndex = m_GroupDataIndex[publisherId][writerId];
                foreach ( int item in groupedDataTypeIndex )
                {
                    if ( item > DetailsDataGridView.Rows.Count )
                    {
                        return;
                    }
                    DataGridViewRow row = DetailsDataGridView.Rows[item];
                    row.HeaderCell.Value = "";
                }
                m_GroupDataIndex[publisherId][writerId]
                       .Clear();
            }
        }

        public static string ToBitString( BitArray bitArray )
        {
            StringBuilder sb                 = new StringBuilder();
            int           insertBlankCounter = 0;
            for ( int i = 0; i < bitArray.Count; i++ )
            {
                char c = bitArray[i] ? '1' : '0';
                sb.Append( c );
                insertBlankCounter++;
                if ( insertBlankCounter > 3 && i != bitArray.Count - 1 )
                {
                    sb.Append( " " );
                    insertBlankCounter = 0;
                }
            }
            return sb.ToString();
        }

        public void UpdateTimeFormatting()
        {
            DataGridViewColumn firstTimeColumn = OverviewDataGridView.Columns["FirstTime"];
            if ( firstTimeColumn != null )
            {
                firstTimeColumn.DefaultCellStyle.Format = "o";
            }
            DataGridViewColumn lastTimeColumn = OverviewDataGridView.Columns["LastTime"];
            if ( lastTimeColumn != null )
            {
                lastTimeColumn.DefaultCellStyle.Format = "o";
            }
            OverviewDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            if ( lastTimeColumn != null )
            {
                lastTimeColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private void ClientOnMessageReceived( object sender, MessageDecodedEventArgs eventargs )
        {
            if ( eventargs.Message == null )
            {
                return;
            }
            if ( InvokeRequired )
            {
                Invoke( new Action<NetworkMessage>( UpdatePropertyGrid ), eventargs.Message );
            }
        }

        private void DetailsDataGridView_DataBindingComplete( object sender, DataGridViewBindingCompleteEventArgs e )
        {
            DataGridViewColumn timeColumn = DetailsDataGridView.Columns["TimeStamp"];
            if ( timeColumn != null )
            {
                timeColumn.DefaultCellStyle.Format = "yyyy-MM-dd-T HH:mm:ss.fffZ";
            }
            DataGridViewColumn originalTimeColumn = DetailsDataGridView.Columns["OriginalTimeStamp"];
            if ( originalTimeColumn != null )
            {
                originalTimeColumn.DefaultCellStyle.Format = "yyyy-MM-dd-T HH:mm:ss.fffZ";
            }
            if ( m_GroupDataIndex.ContainsKey( m_CurrentPublisherId ) == false
              || m_GroupDataIndex[m_CurrentPublisherId]
                        .ContainsKey( m_CurrentWriterId )
              == false )
            {
                return;
            }
            List<int> groupedDataTypeIndex = m_GroupDataIndex[m_CurrentPublisherId][m_CurrentWriterId];
            foreach ( int item in groupedDataTypeIndex )
            {
                if ( item > DetailsDataGridView.Rows.Count )
                {
                    return;
                }
                DataGridViewRow  row = DetailsDataGridView.Rows[item];
                ProcessDataPoint pdp = row.DataBoundItem as ProcessDataPoint;
                if ( pdp.NumberOfSubValues > 0 )
                {
                    row.HeaderCell.Value = "-";
                    if ( pdp.NumberOfSubValues > 0 )
                    {
                        for ( int i = item + 1; i <= item + pdp.NumberOfSubValues; i++ )
                        {
                            DataGridViewRow subrow = DetailsDataGridView.Rows[i];

                            // setting forecolor to white to hide the time of sub value
                            // It is a dirty fix, but no other way, chagning format to the cell is not working
                            if ( timeColumn != null )
                            {
                                subrow.Cells[timeColumn.DisplayIndex]
                                      .Style.ForeColor = Color.White;
                            }
                            if ( originalTimeColumn != null )
                            {
                                subrow.Cells[originalTimeColumn.DisplayIndex]
                                      .Style.ForeColor = Color.White;
                            }
                        }
                    }
                }
            }
        }

        private void DetailsDataGridView_RowHeaderMouseClick_1( object sender, DataGridViewCellMouseEventArgs e )
        {
            int row = e.RowIndex;
            ProcessDataPoint pdp = DetailsDataGridView.Rows[row]
                                                      .DataBoundItem as ProcessDataPoint;
            NodeID id = WellKnownNodeIDs.DefaultEncoding; //TODO: make it right...; ProcessValueFactory.ConvertStringToNodeID(pdp.DataType);
            if ( pdp.NumberOfSubValues > 0 )
            {
                DetailsDataGridView.Rows[row]
                                   .HeaderCell.Value = DetailsDataGridView.Rows[row]
                                                                          .HeaderCell.Value as string
                                                    == "+"
                                                               ? "-"
                                                               : "+";
            }
            for ( int i = 0; i < pdp.NumberOfSubValues; i++ )
            {
                ++row;
                DetailsDataGridView.Rows[row]
                                   .Visible = !DetailsDataGridView.Rows[row]
                                                                  .Visible;
            }
        }

        private void DevicesTreeView_AfterSelect( object sender, TreeViewEventArgs e )
        {
            TreeNode node = e.Node;
            if ( !( node?.Tag is ushort ) )
            {
                return;
            }
            ushort   writerID    = (ushort)e.Node.Tag;
            TreeNode parent      = node.Parent;
            string   publisherID = parent?.Name;
            m_CurrentPublisherId = publisherID;
            m_CurrentWriterId    = writerID;
            if ( string.IsNullOrWhiteSpace( publisherID ) )
            {
                return;
            }
            DetailsDataGridView.AutoGenerateColumns = false;

            //int numberOfColumns = DetailsDataGridView.Columns.Count - 1;
            //for (int i = numberOfColumns; i >= 0; i--)
            //{
            //    DetailsDataGridView.Columns.RemoveAt(i);
            //}
            DetailsDataGridView.DataSource = null;
            DetailsDataGridView.Columns.Clear();
            if ( !m_PublisherBindings.TryGetValue( publisherID, out ConcurrentDictionary<ushort, BindingSource> bsDictionary ) )
            {
                return;
            }
            if ( !bsDictionary.TryGetValue( writerID, out BindingSource bs ) )
            {
                return;
            }
            DetailsDataGridView.AutoGenerateColumns = true;
            DetailsDataGridView.DataSource          = bs;

            //AutoResizeByUser
            DetailsDataGridView.AutoSizeColumnsMode      = DataGridViewAutoSizeColumnsMode.Fill;
            DetailsDataGridView.AllowUserToResizeColumns = true;
        }

        private void InitBinding()
        {
            m_PublisherBindingSource                       =  new BindingSource();
            OverviewDataGridView.AutoGenerateColumns       =  true;
            OverviewDataGridView.AllowUserToAddRows        =  false;
            OverviewDataGridView.AutoSize                  =  true;
            OverviewDataGridView.DataSource                =  m_PublisherBindingSource;
            DetailsDataGridView.AutoGenerateColumns        =  true;
            DetailsDataGridView.AutoSize                   =  true;
            DetailsDataGridView.DataBindingComplete        += DetailsDataGridView_DataBindingComplete;
            DetailsDataGridView.CellContextMenuStripNeeded += OnDetailsGridCellContextMenuNeeded;
            DetailsDataGridView.CellFormatting             += OnDetailsGridCellFormatting;
            DetailsDataGridView.CellToolTipTextNeeded      += OnDetailsGridCellToolTipTextNeeded;
        }

        private void InitTypeConverters()
        {
            Attribute[] typeConverterAttribute = new Attribute[1];
            typeConverterAttribute[0] = new TypeConverterAttribute( typeof(ExpandableObjectConverter) );
            TypeDescriptor.AddAttributes( typeof(NetworkMessageHeader),    typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(ExtendedFlags1),          typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(ExtendedFlags2),          typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(String),                  typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(ChunkedPayloadHeader),    typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(ChunkedMessage),          typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(DataSetFieldFlags),       typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(EnumDataTypes),           typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(EnumField),               typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(FieldMetaData),           typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(KeyValuePair),            typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(LocalizedText),           typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(MetaFrame),               typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(NodeID),                  typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(OptionSet),               typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(StructureField),          typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(StructureDescription),    typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(EnumDescription),         typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(DiscoveryResponseHeader), typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(NetworkMessage),          typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(DataSetFlags1),           typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(DataSetFlags2),           typeConverterAttribute );
            TypeDescriptor.AddAttributes( typeof(DataSetPayloadHeader),    typeConverterAttribute );
        }

        private void OnDetailsGridCellContextMenuNeeded( object sender, DataGridViewCellContextMenuStripNeededEventArgs dataGridViewCellContextMenuStripNeededEventArgs )
        {
            if ( dataGridViewCellContextMenuStripNeededEventArgs.RowIndex == -1 )
            {
                return;
            }
            DataGridViewRow row = null;
            if ( sender is DataGridView view )
            {
                row = view.Rows[dataGridViewCellContextMenuStripNeededEventArgs.RowIndex];
            }
            if ( !( row?.DataBoundItem is FileDataPoint fdp ) )
            {
                return;
            }
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            ToolStripMenuItem saveItem = new ToolStripMenuItem( "Save File" )
                                         {
                                                 Tag = fdp
                                         };
            saveItem.Click += SaveItem_Click;
            contextMenuStrip.Items.Add( saveItem );
            dataGridViewCellContextMenuStripNeededEventArgs.ContextMenuStrip = contextMenuStrip;
        }

        private void OnDetailsGridCellFormatting( object sender, DataGridViewCellFormattingEventArgs e )
        {
            if ( e.Value == null )
            {
                return;
            }
            if ( e.ColumnIndex == CustomColumnIndex || e.ColumnIndex == OrcatColumnIndex )
            {
                byte value = 0;
                if ( byte.TryParse( e.Value.ToString(), out value ) )
                {
                    e.Value             = $"0x{value:X2}";
                    e.FormattingApplied = true;
                }
                return;
            }
            if ( e.ColumnIndex == QualityColumnIndex )
            {
                ushort value = 0;
                if ( ushort.TryParse( e.Value.ToString(), out value ) )
                {
                    e.Value             = $"0x{value:X4}";
                    e.FormattingApplied = true;
                }
                return;
            }
            if ( e.ColumnIndex == MDSPColumnIndex )
            {
                uint value = 0;
                if ( uint.TryParse( e.Value.ToString(), out value ) )
                {
                    e.Value             = $"0x{value:X8}";
                    e.FormattingApplied = true;
                }
            }
        }

        private void OnDetailsGridCellToolTipTextNeeded( object sender, DataGridViewCellToolTipTextNeededEventArgs e )
        {
            if ( e.RowIndex < 0 || e.ColumnIndex < 0 )
            {
                return;
            }
            if ( e.RowIndex > DetailsDataGridView.RowCount || e.ColumnIndex > DetailsDataGridView.ColumnCount )
            {
                return;
            }
            DataGridViewCell cell = DetailsDataGridView.Rows[e.RowIndex]
                                                       .Cells[e.ColumnIndex];
            if ( cell?.Value == null )
            {
                return;
            }
            string   toolTip  = null;
            BitArray bitArray = null;
            if ( e.ColumnIndex == CustomColumnIndex || e.ColumnIndex == OrcatColumnIndex )
            {
                if ( byte.TryParse( cell.Value.ToString(), out byte value ) )
                {
                    bitArray = new BitArray( new[] { value } );
                }
            }
            if ( e.ColumnIndex == QualityColumnIndex )
            {
                if ( ushort.TryParse( cell.Value.ToString(), out ushort value ) )
                {
                    bitArray = new BitArray( BitConverter.GetBytes( value ) );
                }
            }
            if ( e.ColumnIndex == MDSPColumnIndex )
            {
                if ( uint.TryParse( cell.Value.ToString(), out uint value ) )
                {
                    bitArray = new BitArray( BitConverter.GetBytes( value ) );
                }
            }
            if ( bitArray != null )
            {
                if ( BitConverter.IsLittleEndian )
                {
                    ReverseBitArray( bitArray );
                }
                toolTip       = ToBitString( bitArray );
                e.ToolTipText = toolTip;
            }
        }

        private void OnMessageDecoded( object sender, MessageDecodedEventArgs eventargs )
        {
            if ( eventargs.Message == null )
            {
                return;
            }
            if ( InvokeRequired )
            {
                Invoke( new Action<NetworkMessage>( UpdatePropertyGrid ), eventargs.Message );
            }
        }

        private static void ReverseBitArray( BitArray input )
        {
            int length = input.Length;
            int middle = length / 2;
            for ( int i = 0; i < middle; i++ )
            {
                bool bit = input[i];
                input[i] = input[length - i - 1];
                input[length            - i - 1] = bit;
            }
        }

        private void SaveItem_Click( object sender, EventArgs e )
        {
            if ( !( sender is ToolStripMenuItem saveItem ) )
            {
                return;
            }
            if ( !( saveItem.Tag is FileDataPoint fileDataPoint ) )
            {
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog
                                            {
                                                    FileName = fileDataPoint.Name
                                            };
            DialogResult result = saveFileDialog.ShowDialog();
            if ( result != DialogResult.OK )
            {
                return;
            }
            using ( BinaryWriter writer = new BinaryWriter( saveFileDialog.OpenFile() ) )
            {
                writer.Write( fileDataPoint.Content );
                writer.Flush();
            }
        }

        private void UpdatePropertyGrid( NetworkMessage message )
        {
            if ( message is ChunkedMessage chunkedMessage )
            {
                MessagePropertyGrid.SelectedObject = chunkedMessage;
                return;
            }
            if ( message is MetaFrame metaMessage )
            {
                MessagePropertyGrid.SelectedObject = metaMessage;
                m_UpdateValues.MetaMessage         = metaMessage;
                return;
            }
            if ( message is DeltaFrame deltaMessage )
            {
                MessagePropertyGrid.SelectedObject = deltaMessage;
                return;
            }
            if ( message is KeyFrame keyMessage )
            {
                MessagePropertyGrid.SelectedObject = keyMessage;
                return;
            }
            MessagePropertyGrid.SelectedObject = message;
        }

        private void VisualizerForm_FormClosing( object sender, FormClosingEventArgs e )
        {
            m_UpdatePublisher.Stop();
            m_UpdateValues.Stop();
        }
    }
}