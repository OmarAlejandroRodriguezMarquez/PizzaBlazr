using Microsoft.JSInterop;

namespace PizzaBlazor.Client.Helpers
{
    public static class IJSRuntimeMethods
    {
        public static ValueTask<object> GuardarLocalStorage(this IJSRuntime js, string key, string content)
        {
            return js.InvokeAsync<object>("localStorage.setItem", key, content);
        }

        public static ValueTask<object> ObtenerLocalStorage(this IJSRuntime js, string key)
        {
            return js.InvokeAsync<object>("localStorage.getItem", key);
        }

        public static ValueTask<object> RemoverLocalStorage(this IJSRuntime js, string key)
        {
            return js.InvokeAsync<object>("localStorage.removeItem", key);
        }
    }
}
