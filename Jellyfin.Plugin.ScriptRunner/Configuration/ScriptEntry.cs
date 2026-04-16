using System;
using System.Xml.Serialization;

namespace Jellyfin.Plugin.ScriptRunner.Configuration;

[XmlRoot("ScriptEntry")]
public class ScriptEntry
{
    [XmlElement("Id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [XmlElement("Name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("Content")]
    public string Content { get; set; } = string.Empty;

    [XmlElement("DebounceSeconds")]
    public int DebounceSeconds { get; set; } = 30;

    [XmlElement("TriggerOnItemAdded")]
    public bool TriggerOnItemAdded { get; set; } = true;

    [XmlElement("TriggerOnItemUpdated")]
    public bool TriggerOnItemUpdated { get; set; } = false;
}