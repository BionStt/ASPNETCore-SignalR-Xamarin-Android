using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Linq;

namespace App2
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
	public class MainActivity : AppCompatActivity
	{

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.activity_main);

			//Setting up references to views
			var openButton = FindViewById<Button>(Resource.Id.bOpen);
			var closeButton = FindViewById<Button>(Resource.Id.bClose);
			var listView = FindViewById<ListView>(Resource.Id.lvMessages);
			//var sendButton = FindViewById<Button>(Resource.Id.bSend);
			var messageText = FindViewById<EditText>(Resource.Id.etMessageText);
			//var connected = false;

			var messages = new List<StockModel>();
			var arrayAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, messages);
			listView.Adapter = arrayAdapter;

			//creating hub connection
			var hubConnection = new HubConnectionBuilder()
					.WithUrl("http://aspnetcoresignalrangulartypescript.azurewebsites.net/stock")
					.Build();

			// registering a handler
			hubConnection.On<ChatMessage>("ReceiveMessage", (data) =>
			{
				var newMessage = $"{data.user}: {data.message}";
				RunOnUiThread(() => {
					arrayAdapter.Add(newMessage);
					arrayAdapter.NotifyDataSetChanged();
				});
			});

			openButton.Click += async (sender, e) =>
			{
				await hubConnection.StartAsync();
				var channels = await hubConnection.StreamAsChannelAsync<Stock>("StreamStocks");
				while(await channels.WaitToReadAsync())
				{
					Stock sItem = new Stock();
					channels.TryRead(out sItem);
					StockModel model = new StockModel() {  Symbol = sItem.Symbol, Change = sItem.PercentChange, Price = sItem.Price};
					RunOnUiThread(() => {
						
						if(messages.Any(s => s.Symbol == sItem.Symbol))
						{
							foreach (StockModel item in messages)
							{
								if(item.Symbol == model.Symbol)
								{
									decimal change = item.Price - model.Price;
									item.Price = model.Price;
									item.Change = (double)Math.Round(change / item.Price, 4); ;
								}
							}
						}
						else
						{
							messages.Add(model);
						}

						arrayAdapter.Clear();
						arrayAdapter.AddAll(messages);
						arrayAdapter.NotifyDataSetChanged();
					});
				}
			};

			closeButton.Click += async (sender, e) =>
			{
				await hubConnection.StopAsync();
			};
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }
	}
}

