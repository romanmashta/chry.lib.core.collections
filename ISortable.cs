namespace Cherry.Lib.Core.Collections
{
    public interface ISortable
    {
        Accessor SortedBy { get; set; }
        SortDirection Direction { get; set; }
    }
    
    public enum SortDirection{
        None,
        Asc,
        Desc
    }
}