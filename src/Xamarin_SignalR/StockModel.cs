using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace App2
{
	public class StockModel : Java.Lang.Object
	{
		public string Symbol { get; set; }

		public double Change { get; set; }

		public decimal Price { get; set; }

		public override string ToString()
		{
			return Symbol + " --> " + Price + " --> " + Change + "%";
		}
	}
}