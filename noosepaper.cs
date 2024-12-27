#!/usr/local/casperscript/bin/cs -S -I. -sFONTPATH=. --
(common.cs) run common
scriptname (noosepaper) eq {
  /margin where {pop} {/margin 10 def} ifelse
  /lineheight where
    {pop}
    {/lineheight {currentfont font.size 1.5 mul floor} def}
    ifelse
  (starting noosepaper: ) #only #stack
  {sys.argv [2] subarray} stopped
    {pop [(The) (Noose) (gallows.pgm) (Papers)]}
    if
  ( ) string.join gsave /CloisterBlack 47 banner grestore
  (now painting columns) #
  /TimesNewRoman latin1font
  /TimesNewRoman 15 selectfont
  % first subtract margin and lineheight from banner baseline
  %margin sub
  2.5 1  % 2.5 columns, starting at column 1
  {sys.argv 1 get (r) file} stopped {LoremIpsum} if
  (The Federalist Papers) columns
  (discarding word index: ) #only #
  (discarding paragraph count: ) #only #
  (remaining stack: ) #only #stack
  showpage
}
if
% vim: tabstop=8 shiftwidth=2 expandtab softtabstop=2 syntax=postscr
