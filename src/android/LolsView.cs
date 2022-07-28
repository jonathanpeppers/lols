using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using System.Collections.Concurrent;

namespace lols
{
	class LolsView : View
	{
		readonly BlockingCollection<Lol> lols = new();
		readonly float TextSize;

		public LolsView(Context context) : base(context)
		{
			ArgumentNullException.ThrowIfNull(context.Resources);
			TextSize = TypedValue.ApplyDimension(ComplexUnitType.Sp, 14, context.Resources.DisplayMetrics);
		}

		public void AddLol(int width, int height)
		{
			if (lols.Count >= 500)
			{
				lols.Take();
			}
			lols.Add(new Lol(width, height));
			Invalidate();
		}

		protected override void OnDraw(Canvas? canvas)
		{
			base.OnDraw(canvas);

			if (canvas == null)
				return;

			var paint = new Paint();
			paint.TextSize = TextSize;
			foreach (var lol in lols)
			{
				paint.Color = lol.Color;
				canvas.Save();
				canvas.Rotate(lol.Rotation, lol.X, lol.Y);
				canvas.DrawText("lol?", lol.X, lol.Y, paint);
				canvas.Restore();
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
				Color = new Color(random.Next(byte.MaxValue), random.Next(byte.MaxValue), random.Next(byte.MaxValue));
			}

			public int X { get; set; }

			public int Y { get; set; }

			public float Rotation { get; set; }

			public Color Color { get; set; }
		}
	}
}
