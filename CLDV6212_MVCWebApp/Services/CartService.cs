using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;

public class CartService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void AddToCart(CartItem item)
    {
        var cart = GetCart();
        var existingItem = cart.Find(i => i.ProductId == item.ProductId);

        if (existingItem == null)
        {
            cart.Add(item);
        }
        else
        {
            existingItem.Quantity += item.Quantity;
        }

        SaveCart(cart);
    }

    public List<CartItem> GetCart()
    {
        var session = _httpContextAccessor.HttpContext.Session;
        var cartJson = session.GetString("Cart");
        return string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
    }

    public void SaveCart(List<CartItem> cart)
    {
        var session = _httpContextAccessor.HttpContext.Session;
        var cartJson = JsonConvert.SerializeObject(cart);
        session.SetString("Cart", cartJson);
    }

    public void ClearCart()
    {
        _httpContextAccessor.HttpContext.Session.Remove("Cart");
    }
}
