using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using System.Collections.Concurrent;
using Helper = Com.Microsoft.Lols.Helper;

namespace lols
{
	class LolsView : View
	{
		readonly ConcurrentQueue<Lol> lols = new();
		readonly Paint paint = new();

		public LolsView(Context context) : base(context)
		{
			ArgumentNullException.ThrowIfNull(context.Resources);
			paint.TextSize = TypedValue.ApplyDimension(ComplexUnitType.Sp, 14, context.Resources.DisplayMetrics);
		}

		public void AddLol(int width, int height)
		{
			if (lols.Count >= 500)
			{
				lols.TryDequeue(out _);
			}
			lols.Enqueue(new Lol(width, height));
			Invalidate();
		}

		protected override void OnDraw(Canvas? canvas)
		{
			foreach (var lol in lols)
			{
				Helper.Draw(canvas, paint, lol.Color, lol.Rotation, lol.X, lol.Y);
			}
		}

		class Lol
		{
			public Lol(int width, int height)
			{
				var random = Random.Shared;
				Rotation = random.NextSingle() * 360;
				X = random.Next(width);
				Y = random.Next(height);
				Color = (int)(0xFF000000 | random.Next(0xFFFFFF));
			}

			public int X { get; set; }

			public int Y { get; set; }

			public float Rotation { get; set; }

			public int Color { get; set; }
		}
	}
}
