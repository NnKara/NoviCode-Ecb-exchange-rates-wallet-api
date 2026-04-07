using NoviCode.Api.ExceptionHandling;
using NoviCode.Application.Exceptions;
using NoviCode.Domain.Exceptions;
using Xunit;

namespace NoviCode.Infrastructure.Tests.Api;

public sealed class ExceptionMapperTests
{
    [Fact]
    public void Map_NotFoundException_returns_404_not_found()
    {
        var mapped = ExceptionMapper.Map(new NotFoundException("missing"));

        Assert.Equal(404, mapped.StatusCode);
        Assert.Equal("Not Found", mapped.Title);
        Assert.Equal("missing", mapped.Detail);
    }

    [Fact]
    public void Map_ValidationException_returns_400_bad_request()
    {
        var mapped = ExceptionMapper.Map(new ValidationException("bad"));

        Assert.Equal(400, mapped.StatusCode);
        Assert.Equal("Bad Request", mapped.Title);
        Assert.Equal("bad", mapped.Detail);
    }

    [Fact]
    public void Map_DomainValidationException_returns_400_bad_request()
    {
        var mapped = ExceptionMapper.Map(new DomainValidationException("domain bad"));

        Assert.Equal(400, mapped.StatusCode);
        Assert.Equal("Bad Request", mapped.Title);
        Assert.Equal("domain bad", mapped.Detail);
    }

    [Fact]
    public void Map_ConcurrencyException_returns_409_conflict()
    {
        var mapped = ExceptionMapper.Map(new ConcurrencyException("conflict"));

        Assert.Equal(409, mapped.StatusCode);
        Assert.Equal("Conflict", mapped.Title);
        Assert.Equal("conflict", mapped.Detail);
    }

    [Fact]
    public void Map_unknown_exception_returns_generic_500()
    {
        var mapped = ExceptionMapper.Map(new InvalidOperationException("nope"));

        Assert.Equal(500, mapped.StatusCode);
        Assert.Equal("Server Error", mapped.Title);
        Assert.Equal("An unexpected error occurred.", mapped.Detail);
    }
}

