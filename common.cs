#!/usr/local/casperscript/bin/ccs -S -I. --
/common ( -
  definitions generally useful by the noosepaper scripts) docstring {
  /inch {72 mul} def
  /datestamp (DATESTAMP) getenv not {(Today)} if def
  /edition (EDITION) getenv not {(Special Edition)} if def
  /volume (VOLUME) getenv not {(-1.0)} if def
  % split volume number into volume and issue numbers
  2 array volume (.) string.split aload pop /issue exch def /volume exch def
  (common: loading helper scripts) #
  /loremipsum where {pop} {(loremipsum.cs) run} ifelse
  /latin1font where {pop} {(latin1font.cs) run} ifelse
  /paragraphs where {pop} {(paragraphs.cs) run} ifelse
  /urlshorten where {pop} {(urlshorten.cs) run} ifelse
  /pnmimage where {pop} {(pnmimage.cs) run} ifelse
  /columns where {pop} {(columns.cs) run} ifelse
  /banner where {pop} {(banner.cs) run} ifelse
  (common: defining some useful words) #
  /margin where {pop} {/margin 10 def} ifelse
  /red {255 0 0} def
  /green {0 255 0} def
  /blue {0 0 255} def
  /cyan {0 255 255} def
  /yellow {255 255 0} def
  /magenta {255 0 255} def
  /black {0 0 0} def
  /hr ( x y color -  % horizontal rule
    from HTML; for debugging vertical space problems) docstring {
    gsave setrgbcolor pagewidth 2 index dup add sub  % right margin = left
    3 1 roll #stack 0 #stack 3 1 roll #stack moveto #stack rlineto stroke #stack
    grestore
  } bind def
  (common: creating Latin1 fonts) #
  [/TimesNewRoman-Latin1 /Helvetica-Latin1]
    {
      dup findfont /FontName get dup length string cvs (-Latin1) string.endswith
      {
        (creating ) #only dup #
        (-Latin1) name.sub latin1font
      }
      {pop}
      ifelse
    }
    forall
} bind def
scriptname (common) eq {
  common
  (common definitions complete) #
}
if
% vim: tabstop=8 shiftwidth=2 expandtab softtabstop=2 syntax=postscr
