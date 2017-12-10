﻿using System;
using Android.App; 
using Android.Widget;
using Android.OS;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using System.Collections.Generic;
using Android.Locations;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json; 
using System.Linq;
using MapNormal.Droid.Model;

namespace MapNormal.Droid
{
    [Activity(Label = "MapNormal", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {

        GoogleMap map;
        LatLng latLngSource;
        LatLng latLngDestination;
        WebClient webclient;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            SetUpGoogleMap (); 

        }

        void SetUpGoogleMap()
        {
            if ( !CommonHelperClass.FnIsConnected ( this ) )
            {
                Toast.MakeText ( this , Constants.strNoInternet , ToastLength.Short ).Show ();
                return;
            }
            var frag = FragmentManager.FindFragmentById<MapFragment>(Resource.Id.map); 
            var mapReadyCallback = new OnMapReadyClass(); 
            mapReadyCallback.MapReadyAction += delegate(GoogleMap obj )
            {
                map=obj;  
                FnProcessOnMap();
            };  

            frag.GetMapAsync(mapReadyCallback); 
        }


        async void FnProcessOnMap()
        {
            await FnLocationToLatLng();

            FnUpdateCameraPosition(latLngSource);

            if (latLngSource != null && latLngDestination != null)
                FnDrawPath(Constants.strSourceLocation, Constants.strDestinationLocation);
        }

        private void FnUpdateCameraPosition(LatLng pos)
        {
            try
            {
                CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
                builder.Target(pos);
                builder.Zoom(12);
                builder.Bearing(45);
                builder.Tilt(10);
                CameraPosition cameraPosition = builder.Build();
                CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);
                map.AnimateCamera(cameraUpdate);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }
        }

        async Task<bool> FnLocationToLatLng()
        {
            try
            {
            var geo = new Geocoder ( this );  
            var sourceAddress =await geo.GetFromLocationNameAsync(Constants.strSourceLocation , 1 );  
            sourceAddress.ToList ().ForEach ((addr) => {  
                latLngSource =new LatLng(addr.Latitude,addr.Longitude);  
            } ); 

                var destAddress =await geo.GetFromLocationNameAsync(Constants.strDestinationLocation , 1 );  
            destAddress.ToList ().ForEach ((addr) => {  
                latLngDestination =new LatLng(addr.Latitude,addr.Longitude);  
            } ); 

            return true;
            }
            catch
            {
                return false;
            }
        }

        async void FnDrawPath(string strSource, string strDestination)
        {
            string strFullDirectionURL = string.Format(Constants.strGoogleDirectionUrl, strSource, strDestination);
            string strJSONDirectionResponse = await FnHttpRequest(strFullDirectionURL);
            if (strJSONDirectionResponse != Constants.strException)
            {
                RunOnUiThread(() =>
                {
                    if (map != null)
                    {
                        map.Clear();
                        MarkOnMap(Constants.strTextSource, latLngSource,Resource.Drawable.common_full_open_on_phone);
                        MarkOnMap(Constants.strTextDestination, latLngDestination,Resource.Drawable.common_full_open_on_phone);
                    }
                });
                FnSetDirectionQuery(strJSONDirectionResponse);
            }
            else
            {
                RunOnUiThread(() =>
                   Toast.MakeText(this, Constants.strUnableToConnect, ToastLength.Short).Show());
            }
        }

        private void MarkOnMap(string title, LatLng pos, int resourceId)
        {
            RunOnUiThread(() =>
           {
               try
               {
                   var marker = new MarkerOptions();
                   marker.SetTitle(title);
                   marker.SetPosition(pos); //Resource.Drawable.BlueDot
                   marker.SetIcon(BitmapDescriptorFactory.FromResource(resourceId));
                   map.AddMarker(marker);
               }
               catch (Exception ex)
               {
                   Console.WriteLine(ex.Message);
               }
           });
        }

        private async Task<string> FnHttpRequest(string strUri)
        {
            webclient = new WebClient();
            string strResultData;
            try
            {
                strResultData = await webclient.DownloadStringTaskAsync(new Uri(strUri));
                Console.WriteLine(strResultData);
            }
            catch
            {
                strResultData = Constants.strException;
            }
            finally
            {
                if (webclient != null)
                {
                    webclient.Dispose();
                    webclient = null;
                }
            }

            return strResultData;

        }
        string FnHttpRequestOnMainThread(string strUri)
        {
            webclient = new WebClient();
            string strResultData;
            try
            {
                strResultData = webclient.DownloadString(new Uri(strUri));
                Console.WriteLine(strResultData);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                strResultData = Constants.strException;
            }
            finally
            {
                if (webclient != null)
                {
                    webclient.Dispose();
                    webclient = null;
                }
            }

            return strResultData;
        }


        private void FnSetDirectionQuery(string strJSONDirectionResponse)
        {
            var objRoutes = JsonConvert.DeserializeObject<GoogleDirectionClass>(strJSONDirectionResponse);
            //objRoutes.routes.Count  --may be more then one 
            if (objRoutes.routes.Count > 0)
            {
                string encodedPoints = objRoutes.routes[0].overview_polyline.points;

                var lstDecodedPoints = FnDecodePolylinePoints(encodedPoints);
                //convert list of location point to array of latlng type
                var latLngPoints = new LatLng[lstDecodedPoints.Count];
                int index = 0;
                for (int i = 0; i < lstDecodedPoints.Count; i++)
                {
                    Model.Location loc = lstDecodedPoints[i];
                    latLngPoints[index++] = new LatLng(loc.lat, loc.lng);
                }

                var polylineoption = new PolylineOptions();
                polylineoption.InvokeColor(Android.Graphics.Color.Red);
                polylineoption.Geodesic(true);
                polylineoption.Add(latLngPoints);
                RunOnUiThread(() =>
              map.AddPolyline(polylineoption));
            }
        }

        List<Model.Location> FnDecodePolylinePoints(string encodedPoints)
        {
            if (string.IsNullOrEmpty(encodedPoints))
                return null;
            var poly = new List<Model.Location>();
            char[] polylinechars = encodedPoints.ToCharArray();
            int index = 0;

            int currentLat = 0;
            int currentLng = 0;
            int next5bits;
            int sum;
            int shifter;

            try
            {
                while (index < polylinechars.Length)
                {
                    // calculate next latitude
                    sum = 0;
                    shifter = 0;
                    do
                    {
                        next5bits = (int)polylinechars[index++] - 63;
                        sum |= (next5bits & 31) << shifter;
                        shifter += 5;
                    } while (next5bits >= 32 && index < polylinechars.Length);

                    if (index >= polylinechars.Length)
                        break;

                    currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                    //calculate next longitude
                    sum = 0;
                    shifter = 0;
                    do
                    {
                        next5bits = (int)polylinechars[index++] - 63;
                        sum |= (next5bits & 31) << shifter;
                        shifter += 5;
                    } while (next5bits >= 32 && index < polylinechars.Length);

                    if (index >= polylinechars.Length && next5bits >= 32)
                        break;

                    currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);
                    Model.Location p = new Model.Location();
                    p.lat = Convert.ToDouble(currentLat) / 100000.0;
                    p.lng = Convert.ToDouble(currentLng) / 100000.0;
                    poly.Add(p);
                }
            }
            catch
            {
                RunOnUiThread(() =>
                  Toast.MakeText(this, Constants.strPleaseWait, ToastLength.Short).Show());
            }
            return poly;
        }
    }
}