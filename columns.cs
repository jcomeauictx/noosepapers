#!/usr/local/casperscript/bin/cs -S -I. --
% typographicwebdesign.com/setting-text/font-size-line-height-measure-alignment/
% NOTE: if debugging messages are removed, replace each `#` with `pop`
(starting columns.cs) #
/loremipsum where {pop} {(loremipsum.cs) run} ifelse
/latin1font where {pop} {(latin1font.cs) run} ifelse
/paragraphs where {pop} {(paragraphs.cs) run} ifelse
/urlshorten where {pop} {(urlshorten.cs) run} ifelse
/banner where {pop} {(banner.cs) run} ifelse
/margin where {pop} {/margin 10 def} ifelse
/red {255 0 0} def
/green {0 255 0} def
/blue {0 0 255} def
/emdash (\320) def  % only in standard encoding
/hr {  % x y color -  % horizontal rule, for debugging vertical space problems
  gsave setrgbcolor pagewidth 2 index dup add sub  % right margin = left
  3 1 roll 0 3 1 roll moveto rlineto stroke
  grestore
} bind def
/columnline {  % wordlist index - endofparagraph newindex string
  (starting columnline with stack: ) #only #stack
  10 dict begin  % languagelevel 3 here, so dict can grow as needed
  /wordindex exch def
  /maxindex 1 index length 1 sub def
  /line 1024 string def
  {
    wordindex maxindex gt
      {
        (wordindex exceeds maxindex, stack: ) #only #stack
        exit
      }
      {
        %(stack at start of loop: ) #only #stack
        dup wordindex get
        % clean up word, at least by replacing `--` with emdash
        (--) (\320) null string.replace
        dup xwidth spacewidth add
        line string.copy string.truncate  % string with new word appended
        xwidth add  % add to running pixel length total
        %dup (line width after addition would be ) #only #
        linewidth ge wordindex maxindex gt or (stack after ge: ) #only #stack
          {
            % if only one word caused linewidth to be exceeded, this needs
            % to be corrected, or /rangecheck signaled.
            dup xwidth linewidth gt
              {
                dup (http) string.startswith
                {
                  urlshorten
                    {
                      (stack after urlshorten: ) #only #stack
                      1 index exch wordindex exch put
                      (wordlist after shortened URL put: ) #only dup ##
                    }
                    {
                      (urlshorten failed, stack: ) #only #stack
                      /undefinedresult signalerror
                    }
                    ifelse
                }
                {
                  3 sleep  % give chance to see how far it got
                  /rangecheck signalerror
                }
                ifelse
              }
              {
                (discarding ) #only dup ##only pop
                (, exiting loop with stack: ) #only #stack
                exit
              }
              ifelse
          }
          {line strlen 0 gt {line ( ) string.append} if
            line exch string.append /wordindex inc
            %(stack after append: ) #only  #stack
          }
          ifelse
      }
      ifelse
  } loop
  pop  % discard wordlist array
  wordindex maxindex gt  % set endofparagraph flag
  wordindex  % newindex
  line string.truncate  % trim trailing nulls off string
  (stack at end of columnline: ) #only #stack
  end  % local variables dictionary
} bind def

/lineshow {  % string final -
  (before show: ) #only #stack
  {show}  % end of paragraph, don't worry about justification
  { % not end of paragraph, so we have 3 possibilities:
    % (1) a string containing one or more emdashes; use emdash for widthshow;
    % (2) a string containing one or more spaces; use space for widthshow;
    % (3) a single very long word; use ashow.
    dup ( ) string.count dup cvbool  % count the spaces and set flag
      {1 index xwidth linewidth exch sub exch div  % pixels space must occupy
        0 ( ) ord 4 -1 roll (stack before widthshow: ) #only #stack widthshow
      }
      {pop show}
      ifelse
  }
  ifelse
} bind def

/showparagraph {  % x0 y0 y1 wordlist wordindex - wordindex y
  (starting showparagraph with stack: ) #only #stack
  % use local variables to simplify coding
  10 dict begin
  /wordindex exch def /wordlist exch def  /ymin exch def
  /y exch def  /x exch def
  {wordlist wordindex columnline (after columnline: ) #only #stack
    x y moveto 2 index lineshow (after lineshow: ) #only #stack
    /wordindex exch def y lineheight sub /y exch def
    % no need to subtract another line at end, each paragraph is already
    % followed by a 2nd linefeed which will do the job.
    % we're done if it's the end of a paragraph OR column height is exceeded
    y ymin lt or {wordindex y exit} if
  }
  loop
  (stack at end of showparagraph: ) #only #stack
  end  % local variables dictionary
} bind def

/column {  % x0 y0 y1 source wordlist pcount pindex - wordlist pcount pindex
  (creating column with stack: ) #only #stack
  10 dict begin  % for local variables
  /pindex exch def /pcount exch def /wordlist exch def
  /filtered exch def  /y1 exch def  /y exch def  /x exch def
  (source: ) #only filtered ##only (, y1: ) #only y1 #only
  (, y: ) #only y #only (, x: ) #only x #
  /eof false def
  {
    (column loop in progress, wordlist: ) #only wordlist ##only
    (, stack: ) #only #stack  % stack is good to here
    wordlist cvbool not
      {
        (refilling wordlist, stack: ) #only #stack
        filtered  % file object
        1024 16 mul string  % string large enough to hold longest line
        (stack before column loop readline: ) #only #stack
        readline (stack after column loop readline: ) #only #stack
        not /eof exch def  % define local variable eof
        (currentdict: ) #only currentdict ###
        (after readline not /eof exch def, stack: ) #only #stack
        4096 array exch () string.split
        /wordlist exch def  /pindex 0 def
      } if
    x y y1 wordlist pindex showparagraph
    (after showparagraph, stack: ) #only #stack
    /y exch def  /pindex exch def
    pindex wordlist length (all words used? ) #only #stack eq
      {
        /pcount inc  % next paragraph
        /wordlist [] def  % erase wordlist
        /pindex 0 def
      }
      if
    (column: paragraph count so far: ) #only pcount #only
    (, stack: ) #only #stack
    % quit if column complete, or all data processed, or max paragraphs read
    eof {(exiting on EOF, stack: ) #only #stack pop exit} if
    y y1 lt {(exiting on column allocation complete) # exit} if
    pcount MAXPARAGRAPHS eq {(exiting on max paragraphs) # exit} if
    (not exiting, continuing column loop, stack:) #only #stack
  } loop
  wordlist pcount pindex
  (stack at end of column: ) #only ##stack
  end  % end local variables dict
} bind def

/columns {  % ytop columns startcolumn source headline - pcount pindex
  % NOTE: startcolumn is 1-based! using 0 puts first column to left of page!
  (starting columns with stack: ) #only #stack
  % make sure there's a usable font loaded
  currentfont font.size 1.0 le
    {
      /TimesNewRoman-Latin1 findfont /FontName get (-Latin1) name.endswith
        {
          (creating TimesNewRoman-Latin1) #
          /TimesNewRoman latin1font
        }
        if
      % but for now we're using the standard encoding;
      % Latin-1 lacks emdash, among other characters.
      (selecting TimesNewRoman) #
      /TimesNewRoman 14 selectfont
      (TimesNewRoman selected) #
    }
    {
      (using font ) #only currentfont dup /FontName get ##only font.size #
    }
    ifelse
  % the following definitions go into userdict, for use by other routines
  /lineheight {currentfont font.size 1.5 mul floor} def
  (lineheight: ) #only lineheight #
  /MAXPARAGRAPHS 100 def  % NOTE: this is for testing with Lorem ipsum generator
  % NOTE on x and y width (from PLRM section 5.4):
  % Most Indo-European alphabets, including the Latin alphabet,
  % have a positive x width and a zero y width. Semitic alphabets have a
  % negative x width, and some Asian writing systems have a nonzero y width.
  /xwidth {stringwidth pop} bind def
  /spacewidth ( ) xwidth def
  (spacewidth: ) #only spacewidth #
  % broadsheet typically has 5 columns, tabloid 4, zine maybe 2 or 3
  /columnsperpage 3 def
  /columnwidth pagewidth margin sub columnsperpage div def
  /linewidth columnwidth margin sub def
  /columnheight pageheight margin dup add sub def
  (stack before current content top:) #only #stack 10 5 index red hr
  (column width: ) #only columnwidth #only (, height: ) #only columnheight #
  % definitions from here are local to `columns`
  10 dict begin
  /headline exch def
  (source: ) #only dup ##
  (stack before setting up filter: ) #only #stack
  paragraphs filter (stack after setting up filter: ) #only #stack
  /filtered exch def  % removes -file- from stack
  %/defaultdevice cvx 0 .quit  % insert and uncomment this where needed
  % (startcolumn is one-based)
  exch (before zero-basing columnwidth: ) #only #stack
  3 index 1 sub columnwidth mul margin add /x exch def
  /wordlist [] def  % empty so `column` knows to read source
  /pcount 0 def  /pindex 0 def
  2 index ceiling cvi % e.g., 1.5 columns means 2 column width
  (stack after 1 index ceiling cvi: ) #only #stack
  % draw headline here, fontsize according to number of spanned columns
  gsave
    dup  % save copy of rounded-up number of columns
    % select a new font size based on number of columns
    currentfont dup /FontName get exch font.size 3 -1 roll mul .7 mul selectfont
    % show font name and size for debugging
    (currentfont: ) #only currentfont dup /FontName get #only ( ) #only font.size #
    % use fontsize as a proxy for moving to baseline. make it better later.
    (stack before top green line: ) #only #stack margin 4 index green hr
    currentfont font.size 5 index exch sub x exch
    (stack before drawing green line at baseline of headline: ) #only #stack
    margin 1 index green hr
    moveto headline
    (stack before showing headline: ) #only #stack
    1 index columnsperpage lt
      {show}
      {true false centershow}
      ifelse
    (after headline shown: ) #only #stack
    % clear headline before starting column contents
    #stack 4 index lineheight sub #stack 5 swap pop
    (drawing blue line at top of columns) # 10 4 index blue hr
  grestore
  (stack before setting up columns `column` loop: ) #only #stack
  % top of stack (TOS) should be firstcolumn lastcolumn
  2 copy 1 exch  % set up `for` loop, TOS now firstcolumn increment lastcolumn
  {
    (stack at start of columns `column` loop: ) #only #stack
    dup 1 sub columnwidth mul margin add  % starting x of column
    6 index  % starting y of column
    (stack before y1 calculation: ) #only #stack
    6 index  % columns, possibly fractional
    3 index  % column (loop) number
    6 index 1 sub  % start column, zero-based
    (stack before sub in y1 calculation: ) #only #stack
    sub  % number of columns that will be completed once this is drawn
    (stack after 1st sub in y1 calculation: ) #only #stack
    % if start column is 1, we're on column 3, and 2.3 columns are specified,
    % then we need to stop at 0.7 * columnheight.
    % same goes if start column is 2 with 1.3 total columns,
    % or start column 3 and 0.3 total.
    exch sub  % in all the above cases this will yield 0.7
    (stack after exch sub in y1 calculation: ) #only #stack
    dup 0 ge 1 index 1 lt and  % are we on final column?
      {columnheight mul}  % yes, calculate stopheight
      {pop 0}  % otherwise paint all the way down the page
      ifelse
    (stack after y1 calculation: ) #only #stack
    filtered wordlist pcount pindex  % load stack for `column`
    column (after column: ) #only #stack
    /x x columnwidth add def
    /pindex exch def  /pcount exch def  /wordlist exch def
    pop  % loop counter (column number)
  } for
  (stack after columns `for` loop: ) #only #stack
  (discarding final column number: ) #only #
  (discarding initial column number: ) #only #
  (discarding file object: ) #only #
  (discarding number of columns: ) #only #
  (discarding top of columns: ) #only #
  pcount pindex  % leave paragraph count and word index on stack
  % these will be saved for continuation on later pages or in later editions.
  end
} bind def

scriptname (columns) eq {
  (starting columns test program with stack: ) #only #stack
  10 dict begin
  sys.argv dup length 1 gt
    {1 get (r) file}
    {pop LoremIpsum}
    ifelse /datasource exch def
  (bytes available: ) #only datasource bytesavailable #
  pageheight margin dup add sub 2.5 1 datasource (Headline Goes Here) columns
  (now showing columns on page) #
  showpage
  exch (final paragraph shown: ) #only #only (, word index: ) #only #
  (final stack: ) #only ##stack
  end
} if
% vim: tabstop=8 shiftwidth=2 expandtab softtabstop=2 syntax=postscr
