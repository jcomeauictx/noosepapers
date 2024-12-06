# call as: gdb --command=urlshorten.gdb
file /usr/local/casperscript/bin/bccs
break zmkdir
# break again just before `ret`, that's where EIP goes to zero
break *0x0000555555b3c4b9
run -- urlshorten.cs someurl
s 150
