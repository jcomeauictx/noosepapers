#!/usr/local/casperscript/bin/bccs -S -P --
/urllib false import
/urlshorten  ( str#url - str#shorturl bool#true OR str#url bool#false
  generate a short URL for a long one) docstring {
  10 dict begin
  /tries 10 def
  /urldomain (gnixl.com) def
  /urldir (../gnixl/) urldomain string.add def
  /linkdir (l/) def
  /linkpath urldir linkdir string.add def
  /urlprefix urldomain linkdir string.add def
  dup [] urllib.parse.quote linkdir exch string.add
  dup os.path.exists (stack before `mark`: ) #only #stack
    {128 string readlink urlprefix exch add true}
    {
      exch  % put quoted URL out of the way for now
      mark  % make it easy to clean stack regardless of where it failed
      tries  % sufficient number of tries to find a unique random name
        {
          {
            urand 36 10 string cvrs string.lower
            %(stack after creating directory name: ) #only #stack
            linkdir exch string.add dup 8#755
            %(stack before os.mkdir: ) #only #stack
            os.mkdir
            %(stack after os.mkdir: ) #only #stack
          }
          stopped
            {cleartomark mark}
            {true exit}  % mark success with `true`
            ifelse
        }
        repeat
      %(after mkdir: ) #only #stack
      true ne  % failure would leave only a -mark- on stack
        {(failed after ) #only tries #only ( attempts) # false}
        {
          dup (/../../.htaccess) string.add (a) file
          % chop first part of path to form URL
          exch urldir string.removeprefix
          %(after forming URL: ) #only #stack
          (Redirect 301 ) 1 index string.add  % "from" URL added
          %(first part of redirect: ) #only #stack
          ( ) string.add 5 -1 roll string.add %#stack
          (\n) string.add 2 index exch writestring %#stack
          exch closefile %#stack
          exch pop  % discard `mark`
          urldomain exch string.add  % return as full URL except for scheme://
          true
        } ifelse
      %(stack after urlshorten: ) #only #stack
    } ifelse
  end
} bind def

scriptname (urlshorten) eq
  {
    (running urlshorten test program) #
    sys.argv 1 get urlshorten
      {(succeeded, shortened URL: ) #only #}
      {(failed to shorten URL: ) #only #}
      ifelse
  }
  if
% vim: tabstop=8 shiftwidth=2 expandtab softtabstop=2 syntax=postscr
