using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class PluginTag
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<Plugin> Plugins { get; set; }
}
