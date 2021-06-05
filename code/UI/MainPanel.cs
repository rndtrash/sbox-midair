﻿using Sandbox;
using Sandbox.UI;

namespace Instagib.UI
{
	public class MainPanel : Panel
	{
		public string PlayerHealth => Local.Client.Pawn.Health.ToString();

		public MainPanel()
		{
			SetClass( "mainpanel", true );
			SetTemplate( "/InstagibHud.html" );
		}
	}
}