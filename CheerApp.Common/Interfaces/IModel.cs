using System.Reflection;

namespace CheerApp.Common.Interfaces
{
    public interface IModel
    {
        string Id { get; set; }
        string UpdateDateTime { get; set; }
    }
}