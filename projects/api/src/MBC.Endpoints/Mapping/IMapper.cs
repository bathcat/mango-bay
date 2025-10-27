namespace MBC.Endpoints.Mapping;

public interface IMapper<TSource, TDestination>
{
    TDestination Map(TSource source);
}

