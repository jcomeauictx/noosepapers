#!/usr/local/casperscript/bin/ccs --
(loremipsum.cs) run
/EOF (D) ctrl def  % control-D marks end of file
/paragraphs {  % source -
  % 2 consecutive endlines (LF, \012) we will interpret as paragraph
  % replace *single* endline with space (( ), \040)
  % `readline` returns substring true in normal case;
  % substring false at EOF;
  % rangecheck error if string filled before newline seen
  {(stack at start of filter procedure: ) #only #stack
    3 dict begin
    {[
      {counttomark 6 add index  % count back to original -file- object
        8192 string (before readline: ) #only #stack
        readline (after readline: ) #only #stack
        not /eof exch def  % false means end-of-file
        dup strlen 0 eq  % empty string found
          {pop counttomark 0 eq  % only thing found so far?
            {(ignoring empty line preceding actual content) #}
            {pop  % remove space from end of previous line
              (stack at inner loop exit (end of paragraph): ) #only #stack
              exit  % end of paragraph
            }
            ifelse
          }
          {
            % pdftotext replaces end-of-line hyphens with chr(0xac)
            % perhaps it meant chr(0xad), soft hyphen?
            dup strlen 1 sub 2 copy get 16#ac eq %(hyphenated? ) #only #stack
              {0 exch getinterval}  % remove final character
              {pop ( )}  % remove length, and append space
              ifelse
          }
          ifelse
        eof {(exiting inner loop on EOF, stack: ) #only #stack exit} if
      }  % end of paragraph read
      loop
      (joining fragments into paragraph, stack: ) #only #stack
      ]  % create an array of the strings found
      (stack before join: ) #only #stack
      /paragraph 1024 dup mul string def  % megabyte string to hold paragraph
      {paragraph exch string.append} forall paragraph truncate
      (after join complete: ) #only #stack
      dup strlen 0 gt
        {%(adding line separator to concatenated string) #
          (\n\n) string.add
        }
        {(found empty string, marking EOF, stack: ) #only #stack pop EOF}
        ifelse
      exit
    }
    loop
    (exiting outer loop with string ") #only dup #only (") #only
    (, stack: ) #only #stack
    end
  }
  <</EODCount 1 /EODString EOF>>
  /SubFileDecode
} bind def
scriptname (paragraphs) eq {
  (starting paragraphs test program) #
  /count zero
  sys.argv dup length 1 gt
    {1 get (r) file}
    {pop LoremIpsum}
    ifelse
  (testing paragraph reader) #
  {paragraphs filter
    1024 dup mul string
    readline not /eof exch def
    =  % string to stdout
    eof {(exiting on EOF) # exit} if  % quit after all data processed
    /count inc count 10000 eq {exit} if  % quit test after 10000 paragraphs
  } loop
  (stack at end of paragraphs test: ) #only #stack
  (bytes available: ) # bytesavailable #
  (final stack at end of test: ) #only #stack
} if
% vim: tabstop=8 shiftwidth=2 expandtab softtabstop=2 syntax=postscr
