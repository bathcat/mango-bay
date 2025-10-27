using System.Linq;
using MBC.Core.Models;

namespace MBC.Endpoints.Mapping;

public static class PageMapper
{
    public static Page<TDestination> Map<TSource, TDestination>(
        Page<TSource> source,
        IMapper<TSource, TDestination> itemMapper)
    {
        return Page.Create(
            items: source.Items.Select(itemMapper.Map).ToList(),
            offset: source.Offset,
            countRequested: source.CountRequested,
            totalCount: source.TotalCount
        );
    }
}

