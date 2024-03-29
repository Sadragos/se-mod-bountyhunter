﻿using System;
using System.IO;
using System.Text;
using Sandbox.ModAPI;

// This was just pulled from example script, it's a good enough logger

namespace Bountyhunter.Utils
{
    internal class Logging
    {
        private static Logging _instance;
        private readonly StringBuilder _cache = new StringBuilder( );
        private int _indent;

        private readonly TextWriter _writer;

        public Logging( string logFile )
        {
            try
            {
                _writer = MyAPIGateway.Utilities.WriteFileInLocalStorage( logFile, typeof( Logging ) );
                _instance = this;
            }
            catch
            {
            }
        }

        public static Logging Instance
        {
            get
            {
                if ( MyAPIGateway.Utilities == null )
                    return null;

                if ( _instance == null )
                    _instance = new Logging( "bountyhunter.log" );

                return _instance;
            }
        }

        public void IncreaseIndent( )
        {
            _indent++;
        }

        public void DecreaseIndent( )
        {
            if ( _indent > 0 )
                _indent--;
        }

        public void WriteLine( string text )
        {
            try
            {
                if ( _cache.Length > 0 )
                    _writer.WriteLine( _cache );

                _cache.Clear( );
                _cache.Append( DateTime.Now.ToString( "[HH:mm:ss] " ) );
                for ( var i = 0; i < _indent; i++ )
                    _cache.Append( "\t" );

                _writer.WriteLine( _cache.Append( text ) );
                _writer.Flush( );
                _cache.Clear( );
                VRage.Utils.MyLog.Default.WriteLine(text);
            }
            catch
            {
            }
        }

        public void Write( string text )
        {
            _cache.Append( text );
        }


        internal void Close( )
        {
            if ( _cache.Length > 0 )
                _writer.WriteLine( _cache );

            _writer.Flush( );
            _writer.Close( );
        }
    }
}