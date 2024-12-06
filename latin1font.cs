#!/usr/local/casperscript/bin/bccs --
% font modified to use Latin-1 encoding
% PLRM page 349
/latin1font { %  fontname -
  dup
  % add Latin1 character set to font
  findfont
  dup length dict begin {
    1 index /FID ne  % skip font identifier
    {def}  % otherwise copy key-value pair
    {pop pop}  % drop FID
    ifelse
  } forall
  /Encoding ISOLatin1Encoding def
  currentdict end
  %(after currentdict end: ) #only #stack
  1 index 128 string cvs (-Latin1) add cvn dup (new fontname: ) #only ##
  dup (made ) #only #only ( from ) #only 3 -1 roll #
  exch definefont pop  % pop extra copy of font left by `definefont`
} bind def
scriptname (latin1font) eq {
  sys.argv 1 get cvn latin1font
  (stack at end: ) #only #stack
} if
% vim: tabstop=8 shiftwidth=2 expandtab softtabstop=2 syntax=postscr
