namespace Wheelsell.BusinessLogic.DTOs.Common;

public class PagedRequest
{
    private int _page = 1;
    private int _pageSize = 20;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is < 1 or > 100 ? 20 : value;
    }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class ServiceResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }

    public static ServiceResult Ok() => new() { Success = true };
    public static ServiceResult Fail(string error) => new() { Success = false, Error = error };
}

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public T? Data { get; set; }

    public static ServiceResult<T> Ok(T data) => new() { Success = true, Data = data };
    public static ServiceResult<T> Fail(string error) => new() { Success = false, Error = error };
}
