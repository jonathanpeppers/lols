package com.microsoft.lols;

import android.content.Context;
import android.view.ViewGroup;
import android.widget.RelativeLayout;
import android.widget.TextView;

public class Helper {
	public static TextView createTextView (Context context, int color, float rotation, int x, int y)
	{
		TextView textView = new TextView(context);
		textView.setText("lol?");
		textView.setTextColor(color);
		textView.setRotation(rotation);

		RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT);
		layoutParams.leftMargin = x;
		layoutParams.topMargin = y;
		textView.setLayoutParams(layoutParams);

		return textView;
	}

	public static void add(RelativeLayout layout, TextView textView)
	{
		int childCount = layout.getChildCount();
		if (childCount > 500)
			layout.removeViewAt(childCount - 2);
		layout.addView(textView, 0);
	}
}
