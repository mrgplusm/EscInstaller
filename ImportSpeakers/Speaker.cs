﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.5466
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Common;

// 
// This source code was auto-generated by xsd, Version=2.0.50727.3038.
// 

namespace EscInstaller.ImportSpeakers
{




    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [Serializable()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public partial class Speaker
    {
        /// <remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string EQName { get; set; }

        /// <remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Description { get; set; }

        /// <remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string isBosePreset { get; set; }

        /// <remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Limiter { get; set; }

        /// <remarks/>
        [XmlElement("Navigation", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public SpeakerNavigation[] Navigation { get; set; }

        /// <remarks/>
        [XmlElement("Band-pass", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public SpeakerBandpass[] Bandpass { get; set; }

        /// <remarks/>
        [XmlElement("PEQ", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public SpeakerPEQ[] PEQ { get; set; }

        /// <remarks/>
        [XmlElement("Delay", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public SpeakerDelay[] Delay { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string type { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string version { get; set; }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [Serializable()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType = true)]
    public partial class SpeakerNavigation
    {
        /// <remarks/>
        [XmlAttribute()]
        public string level1 { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string level2 { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string level3 { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string level4 { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string label1 { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string label2 { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string label3 { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string label4 { get; set; }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [Serializable()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType = true)]
    public partial class SpeakerBandpass
    {
        /// <remarks/>
        [XmlElement("HighPass", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Bandpass[] HighPass { get; set; }

        /// <remarks/>
        [XmlElement("LowPass", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Bandpass[] LowPass { get; set; }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [Serializable()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType = true)]
    public partial class Bandpass
    {
        private bool _bypass;

        /// <remarks/>
        [XmlAttribute()]
        public double freq { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string filterType { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string bypass
        {
            get { return _bypass.ToString(); }
            set
            {
                bool ParsedValue;

                if (!Boolean.TryParse(value, out ParsedValue))
                    ParsedValue = XmlConvert.ToBoolean(value);

                _bypass = ParsedValue;
            }
        }

        public bool ByPass
        {
            get { return _bypass; }
        }
    }

    ///// <remarks/>
    //[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    //[System.SerializableAttribute()]
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    //public partial class SpeakerBandpassLowPass
    //{
    //    /// <remarks/>
    //    [XmlAttribute()]
    //    public string freq { get; set; }

    //    /// <remarks/>
    //    [XmlAttribute()]
    //    public string filterType { get; set; }

    //    /// <remarks/>
    //    [XmlAttribute()]
    //    public string bypass { get; set; }
    //}

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [Serializable()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType = true)]
    public partial class SpeakerPEQ
    {
        /// <remarks/>
        [XmlElement("Band", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public SpeakerPEQBand[] Band { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string UsingBandwidth { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string EQGain { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string EQPolarity { get; set; }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [Serializable()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType = true)]
    public partial class SpeakerPEQBand
    {

        private FilterType _filterType;

        /// <remarks/>
        [XmlAttribute()]
        public int index { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string type
        {
            get { return _type; }
            set
            {
                if (!SpeakerMethods.FTypes.TryGetValue(value, out _filterType))
                    throw new FileFormatException("Filtertype " + value + " does not exist");

            }
        }

        public FilterType FilterType
        {
            get { return _filterType; }
        }

        /// <remarks/>
        [XmlAttribute()]
        public double freq { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public double gain { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public double width { get; set; }

        private bool _bypass;
        private string _type;

        /// <remarks/>
        [XmlAttribute()]
        public string bypass
        {
            get { return _bypass.ToString(); }
            set
            {
                bool ParsedValue;

                if (!Boolean.TryParse(value, out ParsedValue))
                    ParsedValue = XmlConvert.ToBoolean(value);

                _bypass = ParsedValue;
            }
        }

        public bool ByPass
        {
            get { return _bypass; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [Serializable()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType = true)]
    public partial class SpeakerDelay
    {
        /// <remarks/>
        [XmlAttribute()]
        public string alignmentDelay { get; set; }
    }
}