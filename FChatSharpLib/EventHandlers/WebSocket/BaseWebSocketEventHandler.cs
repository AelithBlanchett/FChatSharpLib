using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Websocket.Client;

namespace FChatSharpLib.Entities.EventHandlers.WebSocket
{
    public abstract class BaseWebSocketEventHandler : IWebSocketEventHandler
    {
        private WebsocketClient _webSocketClient;

        public WebsocketClient WebSocketClient
        {
            get
            {
                return _webSocketClient;
            }

            set
            {
                _webSocketClient = value;
            }
        }

        public bool Debug { get; set; }

        public BaseWebSocketEventHandler()
        {
        }

        private static string _PUBLICKEY = "3082010A0282010100AD211461B695F2BF129B4854D64C405E9A5A282F50960704AF0DAC3E5D2D5F870B5BF43E646307D10EDA26282E2915E2772516524353BB9B2DFA66610FBC66612907532E1BEC05A520154E2A7EE4C8D0C9566D8AFE175CBA6E0F29CE48C4089B903520D81B6EA6C6658ECC278017241216DDAF8A785660DD1042A83960488DCDB3A1DEFAEAF61867B17FBBEB9033EB84F2ABA9E26CB0FC99D07E1BBA9919DE5A0FA8E20C014792382F2127881D0D1E9C7A26F035317FFCA8E8E4BA8C14BAB1E67D4E6EE42D7C48B65E9E2DB86A624212D46DA911038B73D2C40C8E64D94755D54AD0E729BC4C70289E8E1BF7ECCD09967E88F8C385FF7D7B44A1EBE8EB5C4D430203010001";

        public static bool ValidateServerCertificateWithPublicKey(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return certificate.GetPublicKeyString().Equals(_PUBLICKEY.ToUpper());
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers unless it's F-list's public key.
            return ValidateServerCertificateWithPublicKey(sender, certificate, chain, sslPolicyErrors);
        }

        public void InitializeWsClient(string url, int delayBeforeReconnectInMs)
        {
            if (_webSocketClient == null)
            {
                var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
                {
                    Options =
                    {
                        RemoteCertificateValidationCallback = ValidateServerCertificate,
                        KeepAliveInterval = TimeSpan.FromSeconds(3)
                    }
                });

                _webSocketClient = new WebsocketClient(new Uri(url), factory);
                ListenToWsEvents(_webSocketClient, delayBeforeReconnectInMs);
                _webSocketClient.ReconnectionHappened.Subscribe(info => OnOpen(this, null));
            }
        }

        public void ListenToWsEvents(WebsocketClient websocketClient, int delayBeforeReconnectInMs)
        {
            _webSocketClient.ReconnectTimeout = TimeSpan.FromMilliseconds(delayBeforeReconnectInMs);
            _webSocketClient.ErrorReconnectTimeout = TimeSpan.FromMilliseconds(delayBeforeReconnectInMs*4);
            _webSocketClient.DisconnectionHappened.Subscribe(type => this.OnClose(this, type));
            _webSocketClient.MessageReceived.Subscribe(type => this.OnMessage(this, type));
        }

        public abstract void OnOpen(object sender, EventArgs e);

        public abstract void OnClose(object sender, DisconnectionInfo e);

        public abstract void OnError(object sender, DisconnectionInfo e);

        public abstract void OnMessage(object sender, ResponseMessage e);

        public abstract void Close();

        public abstract void Connect();
    }
}
