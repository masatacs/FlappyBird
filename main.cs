using System;
using System.Drawing;
using System.Collections.Generic;

using DxLibDLL;

class Sprite
{
	public Rectangle mPos;
	public int[] mGH;
	
	public Sprite( int[] gh, Rectangle pos )
	{
		mPos = pos;
		mGH = gh;
	}
}

class Pipe : Sprite
{
	public static readonly int PIPE_WIDTH = 100;
	public static readonly int PIPE_HEIGHT = 300;
	
	public Rectangle[] mCol;
	
	int padding = 120;
	
	public Pipe( int[] gh, Rectangle pos ) : base( gh, pos )
	{
		mCol = new Rectangle[ 2 ];
	
		mCol[ 0 ] = new Rectangle( pos.X - pos.Width / 2, pos.Y - pos.Height / 2, mPos.Width, mPos.Height );
		mCol[ 1 ] = new Rectangle( pos.X - pos.Width / 2, pos.Y - pos.Height / 2 + pos.Height + padding, mPos.Width, mPos.Height );
	}
	
	void move( int dx, int dy ) 
	{
		mPos.X += dx;
		mPos.Y += dy;
		
		for( int i = 0; i < mCol.Length; i++ ) {
			mCol[ i ].X += dx;
			mCol[ i ].Y += dy;
		}
	}
	
	public void Update()
	{
		move( -3, 0 );
	}
	
	public void DrawSprite( int idx )
	{
		DX.DrawRotaGraph( mPos.X, mPos.Y, 1, Math.PI, mGH[ idx ], DX.TRUE );
		DX.DrawRotaGraph( mPos.X, mPos.Y + mPos.Height + padding, 1, 0, mGH[ idx ], DX.TRUE );
	}
}

class Player : Sprite
{
	public Rectangle mCol;

	int velY;
	int keyUp;
	
	public Player( int[] gh, Rectangle pos, Rectangle col ) : base( gh, pos )
	{
		mCol = col;
	}
	
	void move( int dx, int dy )
	{
		mPos.X += dx;
		mPos.Y += dy;
		
		mCol.X += dx;
		mCol.Y += dy;
	}
	
	public void Update()
	{
		if( DX.CheckHitKey( DX.KEY_INPUT_UP ) == DX.TRUE ) {
			keyUp++;
		} else {
			keyUp = 0;
		}
		
		if( keyUp == 1 ) {
			velY = -20;
		}
		
		move( 0, velY >> 2 );
		velY++;
		
		if( mPos.Y >= Program.WINDOW_HEIGHT ) {
			Program.mGameOver = true;
		}
	}
	
	public void DrawSprite( int idx )
	{
		DX.DrawRotaGraph( mPos.X, mPos.Y, 1, velY * Math.PI / 180, mGH[ idx ], DX.TRUE );
	}
}

class Program
{
	public static readonly int WINDOW_WIDTH = 500;
	public static readonly int WINDOW_HEIGHT = 400;
	
	public static readonly int BIRD_TILE = 50;
	
	public static readonly int ADD_INTERVAL = 90;
	public static readonly int INTERVAL = 16;
	
	public static bool mGameOver;
	public static int mScoer;
	
	static int[] backGroundTile;
	static int[] birdTile;
	static int[] pipeTile;
	
	static int mTimer;
	
	public static Player player;
	static List<Pipe> pipe;
	
	static int frame;

	static void Init()
	{
		DX.SetOutApplicationLogValidFlag( DX.FALSE );
		DX.ChangeWindowMode( DX.TRUE );
		DX.SetGraphMode( WINDOW_WIDTH, WINDOW_HEIGHT, 32 );
		DX.DxLib_Init();
		DX.SetDrawScreen( DX.DX_SCREEN_BACK );
	}
	
	static void Load()
	{
		birdTile = new int[ 1 ];
		DX.LoadDivGraph( "img/bird_1.png", 1, 1, 1, BIRD_TILE, BIRD_TILE, birdTile );
		
		backGroundTile = new int[ 1 ];
		DX.LoadDivGraph( "img/back_ground.png", 1, 1, 1, WINDOW_WIDTH, WINDOW_HEIGHT, backGroundTile );
		
		pipeTile = new int[ 1 ];
		DX.LoadDivGraph( "img/pipe.png", 1, 1, 1, Pipe.PIPE_WIDTH, Pipe.PIPE_HEIGHT, pipeTile );
	}
	
	static void Start()
	{
		DX.SetFontSize( 32 );
	
		player = new Player( 
			birdTile, 
			new Rectangle( BIRD_TILE, WINDOW_HEIGHT / 2, BIRD_TILE, BIRD_TILE ),
			new Rectangle( BIRD_TILE - 16, WINDOW_HEIGHT / 2 - 16, 40, 35 )
		);
		
		pipe = new List<Pipe>();
	}
	
	static void Loop()
	{
		mTimer = DX.GetNowCount();
		
		while( DX.ProcessMessage() >= 0 ) {
			Update();
			Draw();
		
			mTimer += INTERVAL;
			DX.WaitTimer( Math.Max( 1, mTimer - DX.GetNowCount() ) );
		}
	}
	
	static void Update()
	{
		if( !mGameOver ) {
			player.Update();
		}
		
		for( int i = 0; i < pipe.Count; i++ ) {
			pipe[ i ].Update();
			
			for( int n = 0; n < pipe[ i ].mCol.Length; n++ ) {
				if( pipe[ i ].mCol[ n ].IntersectsWith( player.mCol ) ) {
					mGameOver = true;
					break;
				}
				
				if( pipe[ i ].mCol[ n ].X + pipe[ i ].mCol[ n ].Width < 0 && !mGameOver ) {
					pipe.RemoveAt( i );
					mScoer++;
				}
			}
		}
		
		frame--;
		
		if( frame < 0 ) {
			Random rdm = new Random();
			int y = rdm.Next( -100, 100 );

			pipe.Add( new Pipe( 
				pipeTile,
				new Rectangle( WINDOW_WIDTH, y, Pipe.PIPE_WIDTH, Pipe.PIPE_HEIGHT ) 
			));
			
			frame = ADD_INTERVAL;
		}
	}
	
	static void Draw()
	{
		int fontWidth;
	
		DX.ClearDrawScreen();

		DrawSprite( 0, 0, WINDOW_WIDTH, WINDOW_HEIGHT, backGroundTile[ 0 ], DX.TRUE );
				
		for( int i = 0; i < pipe.Count; i++ ) {
			pipe[ i ].DrawSprite( 0 );
		}
		
		player.DrawSprite( 0 );
		
		fontWidth = DX.GetDrawStringWidth( mScoer.ToString(), -1 );
		DX.DrawString( ( WINDOW_WIDTH - fontWidth ) / 2, 32, mScoer.ToString(), DX.GetColor( 0xff, 0xff, 0xff ) );
		
		if( mGameOver ) {
			fontWidth = DX.GetDrawStringWidth( "GAME OVER", -1 );
			DX.DrawString( ( WINDOW_WIDTH - fontWidth ) / 2, WINDOW_HEIGHT / 2 - 32, "GAME OVER", DX.GetColor( 0xff, 0xff, 0xff ) );
		}
		
		DX.ScreenFlip();
	}
	
	public static void DrawSprite( int x, int y, int w, int h, int t, int a )
	{
		DX.DrawExtendGraph( x, y, w, h, t, a );
	}

	static void Main()
	{
		Init();
		Load();
		Start();
		Loop();
		
		DX.DxLib_End();
	}
}
