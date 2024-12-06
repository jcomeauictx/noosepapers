#!/usr/local/casperscript/bin/cs --
% insert pnm image at currentpoint
% code stolen from casperscript/vdiff.cs

/inch {72 mul} bind def
/asciizero (0) 0 get def  % value of ASCII zero (0x30)

/bits <<  % translate maxvalue into bits per sample
  1 1
  255 8
>> def

/decode <<  % Pn type into Decode matrix
  /P1 [0 1]
  /P2 [0 1]
  /P3 [0 1 0 1 0 1]
>> def

/binarize <<  % Pn type into binary stream
  /P1 {
    % read until (0), (1), or EOF
    {infile read
      {dup 1 not and asciizero eq
        {asciizero - 1 string dup 0 4 -1 roll put exit}
        {pop}
        ifelse}
      {() exit}
      ifelse
    } loop
  }
  /P2 {infile token {1 string dup 0 4 -1 roll put} {()} ifelse}
  /P3 {infile token {1 string dup 0 4 -1 roll put} {()} ifelse}
>> def

/colorspaces <<  % Pn type into colorspace
  /P1 /DeviceGray
  /P2 /DeviceGray
  /P3 /DeviceRGB
>> def

/pnm <<  % valid type 1 image dict once fleshed out
  /ImageType 1
  /Height 0
  /Width 0
  /ImageMatrix [/w 0 0 /-h 0 /h]  % placeholder for top-bottom left-right data
  /MultipleDataSources false  % optional (default) but let's be explicit
  /DataSource null  % replace with input stream
  /BitsPerComponent 1  % default for P1
  /Decode [0 1]  % map colors into range 0 to 1 ([0 1 0 1 0 1] for RGB)
  /Interpolate false  % another optional, default false
>> def

/comment (#) 0 get def  % comment introducer

/readpnm {  % filename - pnminstance colorspace
  % read PNM file into dict
  % we confine ourselves to gs-created pnm files, which only have a single
  % whole-line comment on 2nd line. this won't work with end-of-line comments.
  % netpbm programs pngtopnm and pnmnoraw don't insert such comments,
  % and this should work with those as well.
  pnm dup length dict copy /instance exch def  % new instance of pnm dictionary
  (r) file /infile exch def  % store open file handle in `infile`.
  /buffer 1024 string def  % hopefully enough for any PNM line length
  /pnmtype infile token pop cvlit def  % /P1, /P2, or /P3
  instance /Decode decode pnmtype get put
  /colorspace colorspaces pnmtype get def
  % read and discard the expected comment.
  % if not a comment, parse the width and height
  (stack before processing 2nd line of PNM file: ) #only #stack
  infile buffer readline pop dup 0 get comment ne
    { (not a comment, parsing width and height from string) #
      token pop instance /Width 3 -1 roll cvi dup /width exch def put
      token pop instance /Height 3 -1 roll cvi dup /height exch def put
      pop  % now-empty string
    }
    { (comment found, toss it and parse width and height from infile) #
      pop  % discard comment
      instance /Width infile token pop cvi dup /width exch def put
      instance /Height infile token pop cvi dup /height exch def put
    }
  ifelse
  (stack remaining after processing 2nd line: ) #only #stack
  instance /ImageMatrix get dup dup  % we're going to overwrite the /w etc.
    0 width put
    3 height neg put
    5 height put
  pnmtype (P1) ne {  % P2 and P3 have an extra line, max value
    instance /BitsPerComponent bits infile token pop cvi get put
  } if
  instance /DataSource binarize pnmtype get /ReusableStreamDecode filter put
  instance dup (instance: ) #only ### colorspace
} bind def

/pnmimage {  % filename -
  % insert image from pnm (pgm, pbm) file on page at currentpoint
  readpnm setcolorspace dup dup /Width get exch /Height get
  currentpoint translate scale image
} def

/pnmtopage {  % filename -
  % insert image from pnm (pgm, pbm) file to fit page height
  readpnm setcolorspace dup dup /Width get exch /Height get pop pop  % discard
  0.5 inch 0.5 inch moveto currentpoint translate
  pagesize 1 inch sub exch 1 inch sub exch scale image
} def

/fontheight {  % string - totalheight descender  % descender typically negative
  % NOTE: must use `newpath` because `moveto` only appends to current path
  % (it wasn't scaling image properly when moving from lower on the page)
  gsave newpath 0 0 moveto false charpath flattenpath [ pathbbox ]
  dup 1 get  % lower left y
  exch 3 get  % upper right y
  1 index sub  % total height ury-lly
  exch  % return in correct order
  grestore
} bind def

/pnminline {  % filename string -
  % draw image in correct aspect ratio using string as height limits
  % don't put descenders in string if you want image on baseline of font
  (pathbbox at start of pnminline: ) #only [pathbbox] ##
  (stack at start of pnminline: ) #only #stack
  fontheight dup (fontheight: ) #only #
  (stack after fontheight: ) #only #stack
  gsave 0 exch rmoveto  % adjust y to that of string given
  (stack after 0 y removeto: ) #only #stack
  exch readpnm setcolorspace dup dup /Width get exch /Height get
  (desiredheight imagedict imagewidth imageheight: ) #only #stack
  3 index exch div mul  % multiply image width by height ratio
  (X adjustment for scaled image in pixels: ) #only dup #
  dup 4 1 roll  % save width to adjust x after image
  (stack before `3 -1 roll`: ) #only #stack
  3 -1 roll (stack before `currentpoint translate scale image`: ) #only #stack
  currentpoint translate scale image
  (stack before grestore dup: ) #only #stack
  grestore
  dup 128 string (moving %.2f pixels to the right)
    3 -1 roll (stack before 1 array astore: ) #only #stack
    1 array astore sprintf pop #
  (stack before 0 rmoveto: ) #only #stack 0 rmoveto
  (pathbbox at end of pnminline: ) #only [pathbbox] ##
  (stack at end of pnminline: ) #only #stack
} def
  
% test run using `cs -- pnmimage.cs gallows.pgm`
scriptname (pnmimage) eq {
  sys.argv 1 get pnmtopage
  showpage
} if
% vim: tabstop=8 shiftwidth=2 expandtab softtabstop=2 syntax=postscr
