using System;
namespace MapNormal.Droid.Model
{
    static class Constants
    {
        internal static string strGoogleServerKey = "AIzaSyDh9zKS17TmkNPnuT5oKosLhgj9jxNimIA";
        internal static string strGoogleServerDirKey = "AIzaSyCL4efArxSiLygzZq0k0fQB5qYuDehlU7g";
        internal static string strGoogleDirectionUrl = "https://maps.googleapis.com/maps/api/directions/json?origin={0}&destination={1}&key=" + strGoogleServerDirKey + "";
        internal static string strGeoCodingUrl = "https://maps.googleapis.com/maps/api/geocode/json?{0}&key=" + strGoogleServerKey + "";
        internal static string strSourceLocation = "Latkrabang,Bangkok";
        internal static string strDestinationLocation = "King Mongkut's Institute of Technology Ladkrabang, Thanon Chalong Krung, Khwaeng Lam Prathew, Khet Lat Krabang, Krung Thep Maha Nakhon 10520";

        internal static string strException = "Exception";
        internal static string strTextSource = "Source";
        internal static string strTextDestination = "Destination";

        internal static string strNoInternet = "No online connection. Please review your internet connection";
        internal static string strPleaseWait = "Please wait...";
        internal static string strUnableToConnect = "Unable to connect server!,Please try after sometime";
    }
}
