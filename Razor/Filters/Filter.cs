using System;
using System.Collections;
using System.Windows.Forms;
using System.Xml;

namespace Assistant.Filters
{
    public abstract class Filter
    {
        private static readonly ArrayList m_Filters = new ArrayList();

        public static ArrayList List
        {
            get { return m_Filters; }
        }

        public static void Register(Filter filter)
        {
            m_Filters.Add(filter);
            filter.OnEnable();
        }

        public static void UnRegister(string filterName)
        {
            int removeit = -1;
            for (int i = 0; i < m_Filters.Count; i++)
            {
                if (filterName == ((Filter)m_Filters[i]).Name)
                {
                    ((Filter)m_Filters[i]).OnDisable();
                    removeit = i;
                    break;
                }
            }
            if (removeit > -1)
            {
                m_Filters.RemoveAt(removeit);
            }
        }

        internal static void Load()
        {
            DisableAll();
            foreach (Filter f in m_Filters)
            {
                if (RazorEnhanced.Settings.General.ReadBool(f.Name))
                    f.OnEnable();
            }
        }

        public static void DisableAll()
        {
            for (int i = 0; i < m_Filters.Count; i++)
                ((Filter)m_Filters[i]).OnDisable();
        }

        public static void Save(XmlTextWriter xml)
        {
            for (int i = 0; i < m_Filters.Count; i++)
            {
                Filter f = (Filter)m_Filters[i];
                if (f.Enabled)
                {
                    xml.WriteStartElement("filter");
                    xml.WriteAttributeString("name", f.Name);
                    xml.WriteAttributeString("enable", f.Enabled.ToString());
                    xml.WriteEndElement();
                }
            }
        }

        public static void Draw(CheckedListBox list)
        {
            list.BeginUpdate();
            list.Items.Clear();

            for (int i = 0; i < m_Filters.Count; i++)
            {
                Filter f = (Filter)m_Filters[i];
                list.Items.Add(f);
                list.SetItemChecked(i, f.Enabled);
            }

            list.EndUpdate();
        }

        public abstract void OnFilter(PacketReader p, PacketHandlerEventArgs args);
        public abstract byte[] PacketIDs { get; }
        public abstract string Name { get; }

        public bool Enabled
        {
            get { return m_Enabled; }
        }

        private bool m_Enabled;
        private readonly PacketViewerCallback m_Callback;

        protected Filter()
        {
            m_Enabled = false;
            m_Callback = new PacketViewerCallback(this.OnFilter);
        }

        public override string ToString()
        {
            return this.Name;
        }

        public virtual void OnEnable()
        {
            m_Enabled = true;
            for (int i = 0; i < PacketIDs.Length; i++)
                PacketHandler.RegisterServerToClientViewer(PacketIDs[i], m_Callback);
        }

        public virtual void OnDisable()
        {
            m_Enabled = false;
            for (int i = 0; i < PacketIDs.Length; i++)
                PacketHandler.RemoveServerToClientViewer(PacketIDs[i], m_Callback);
        }

        public void OnCheckChanged(CheckState newValue)
        {
            if (Enabled && newValue == CheckState.Unchecked)
            {
                OnDisable();
                RazorEnhanced.Settings.General.WriteBool(this.Name, false);
            }
            else if (!Enabled && newValue == CheckState.Checked)
            {
                OnEnable();
                RazorEnhanced.Settings.General.WriteBool(this.Name, true);
            }
        }
    }
}
