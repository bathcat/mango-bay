namespace MBC.Endpoints.Mapping;

public static class MapperExtensions
{
    public static TDestination? MapOptional<TSource, TDestination>(this IMapper<TSource, TDestination> mapper, TSource? source)
        where TDestination : class
    {
        if (source == null)
        {
            return null;
        }

        return mapper.Map(source);
    }
}

