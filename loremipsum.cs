#!/usr/local/casperscript/bin/ccs --
/loremipsum  % the basic string
(Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.) def

/LoremIpsum  % file-like object that produces infinite copies of the string
  {loremipsum (\n\n) string.add}
  <</EODCount 0 /EODString()>>
  /SubFileDecode filter
  def

scriptname (loremipsum) eq {
  LoremIpsum {dup 256 string readstring {print} if} loop
} if
% vim: tabstop=8 shiftwidth=2 expandtab softtabstop=2 syntax=postscr
