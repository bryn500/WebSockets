using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

namespace WebSockets.Controller
{
    public class WebSocketController : ControllerBase
    {
        private DateTime LastCheck = DateTime.Now;

        [Route("/ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await HandleConnection(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        [Route("/timer")]
        public async Task GetTimer()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await HandleConnection(webSocket, true);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task HandleConnection(WebSocket webSocket, bool isTimer = false)
        {
            var buffer = new byte[1024 * 4];
            var clientBuffer = new ArraySegment<byte>(new byte[1024 * 4]);
            var receiveResult = await webSocket.ReceiveAsync(clientBuffer, CancellationToken.None);

            if (clientBuffer.Array == null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await CloseConnection(webSocket, receiveResult);
                return;
            }

            if (isTimer)
                await CreatePushHandler(webSocket, receiveResult);
            else
                await CreateResponseHandler(webSocket, buffer, clientBuffer, receiveResult);

            await CloseConnection(webSocket, receiveResult);
        }

        private async Task CreateResponseHandler(WebSocket webSocket, byte[] buffer, ArraySegment<byte> clientBuffer, WebSocketReceiveResult receiveResult)
        {
            while (!receiveResult.CloseStatus.HasValue)
            {
                string message = Encoding.UTF8.GetString(clientBuffer.Array, clientBuffer.Offset, receiveResult.Count);

                switch (message)
                {
                    case "test":
                        await SendTextResponse(webSocket, receiveResult, Encoding.UTF8.GetBytes("{ 'custom': 'response' }"));
                        break;
                    default:
                        await SendTextResponse(webSocket, receiveResult, new ArraySegment<byte>(buffer, 0, receiveResult.Count));
                        break;
                }

                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }
        }

        private async Task CreatePushHandler(WebSocket webSocket, WebSocketReceiveResult receiveResult)
        {
            while (!receiveResult.CloseStatus.HasValue)
            {
                if (LastCheck < DateTime.Now.AddSeconds(-10))
                {
                    string response = DateTime.Now.ToString("o");
                    byte[] bytes = Encoding.UTF8.GetBytes(response);
                    await SendTextResponse(webSocket, receiveResult, bytes);
                    LastCheck = DateTime.Now;
                }
            }
        }

        private async Task SendTextResponse(WebSocket webSocket, WebSocketReceiveResult receiveResult, ArraySegment<byte> bytes)
        {
            await webSocket.SendAsync(
                    bytes,
                    WebSocketMessageType.Text,
                    receiveResult.EndOfMessage,
                    CancellationToken.None);
        }

        private async Task CloseConnection(WebSocket webSocket, WebSocketReceiveResult receiveResult)
        {
            await webSocket.CloseAsync(
                            receiveResult.CloseStatus.Value,
                            receiveResult.CloseStatusDescription,
                            CancellationToken.None);
        }
    }
}
