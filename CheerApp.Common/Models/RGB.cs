namespace CheerApp.Common
{
	public class RGB
	{
		public RGB(int r, int g, int b, int a)
		{
			R = r;
			G = g;
			B = b;
            A = a;
        }

		public int R { get; set; }
		public int G { get; set; }
		public int B { get; set; }
        public int A { get; set; }
    }
}
