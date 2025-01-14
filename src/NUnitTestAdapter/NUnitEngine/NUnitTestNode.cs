﻿using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Xml;

namespace NUnit.VisualStudio.TestAdapter.NUnitEngine
{
    public interface INUnitTestNode
    {
        string Id { get; }
        string FullName { get; }
        string Name { get; }
        IEnumerable<NUnitProperty> Properties { get; }
        string Seed { get; }
    }

    public abstract class NUnitTestNode : INUnitTestNode
    {
        protected XmlNode Node { get; set; }  // Need to be protected, but still the outputnodes are XmlNode
        public virtual string Id
        {
            get
            {
                string id = Node.GetAttribute("id");
                if (id == null)
                    return id;
                if (!id.StartsWith("1") && char.IsDigit(id[0]))
                    id = id.Length > 1 ? "1" + id.Substring(1) : "1";
                return id;
            }
        }

        public string FullName => Node.GetAttribute("fullname");
        public string Name => Node.GetAttribute("name");

        public bool IsNull => Node == null;

        private readonly List<NUnitProperty> properties = new ();
        public IEnumerable<NUnitProperty> Properties => properties;
        public string Seed => Node.GetAttribute("seed");

        protected NUnitTestNode(XmlNode node)
        {
            Node = node;
            var propertyNodes = Node.SelectNodes("properties/property");
            if (propertyNodes != null)
            {
                foreach (XmlNode prop in propertyNodes)
                {
                    properties.Add(new NUnitProperty(prop.GetAttribute("name"), prop.GetAttribute("value")));
                }
            }
        }
    }
}