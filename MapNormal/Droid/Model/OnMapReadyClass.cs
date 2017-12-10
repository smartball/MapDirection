using System;
using Android.Gms.Maps;

namespace MapNormal.Droid.Model
{
    public class OnMapReadyClass: Java.Lang.Object, IOnMapReadyCallback
    {
        public GoogleMap Map { get; private set; }
        public event Action<GoogleMap> MapReadyAction;
        public void OnMapReady(GoogleMap googleMap)
        {
            Map = googleMap;
            if (MapReadyAction != null)
                MapReadyAction(Map);
        }
    }
}
