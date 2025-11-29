using BookCatalog.Application.DTOs.Books.Responses;
using CartAndWishlist.BLL.Features.CartItems.DTOs.Responses;
using CartAndWishlist.BLL.Features.WishlistItem.DTOs.Responses;
using Shared.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHttpClient("order-inventory-api", client =>
{
    client.BaseAddress = new Uri("http://order-inventory-api");
});

builder.Services.AddHttpClient("book-catalog-api", client =>
{
    client.BaseAddress = new Uri("http://book-catalog-api");
});

builder.Services.AddHttpClient("cart-wishlist-api", client =>
{
    client.BaseAddress = new Uri("http://cart-wishlist-api");
});

var app = builder.Build();
app.MapDefaultEndpoints();

app.MapGet("/api/full-book-data/{bookId:guid}", async (
    IHttpClientFactory httpClientFactory,
    Guid bookId) =>
{

    var catalogClient = httpClientFactory.CreateClient("book-catalog-api");
    var cartWishlistClient = httpClientFactory.CreateClient("cart-wishlist-api");
    
    var book = await catalogClient.GetFromJsonAsync<BookDto>($"api/books/{bookId}");
    var cartItems = await cartWishlistClient
        .GetFromJsonAsync<List<CartItemDto>>($"api/cart-items/by-book/{bookId}");
    
    return Results.Ok(new
    {
        book,
        cartItems,
    });
});


app.Run();