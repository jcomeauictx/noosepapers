SHELL := /bin/bash
SERVER := $(notdir $(PWD))
DOCROOT := /var/www
DRYRUN ?= --dry-run
PATH := /usr/local/casperscript/bin:/usr/local/poppler/bin:$(PATH)
TIMESTAMP := $(shell date +'%Y%m%d%H%M%S')
DATESTAMP := $(shell date +'%A, %B %e, %Y' | sed 's/ \+/ /g')
VOLUME ?= $(shell cat volume.txt 2>/dev/null || echo 0.0)
EDITION ?= Petaluma, California
PDFS := $(shell ls -r *.pdf)
TXTS := $(PDFS:.pdf=.txt)
# manually make default.txt symlink to test text (other than lorem ipsum)
TEXTFILE ?= default.txt
BANNER ?= The Noose gallows.pgm Papers
ifneq ($(SHOWENV),)
export
else
export DATESTAMP VOLUME EDITION
endif
all: gallows.pgm default.txt noosepaper.log
default.txt:
	@echo Manually symlink a .txt file to be default.txt >&2
	false
gallows.pgm: gallows.png
	pngtopnm $< | pnmnoraw > $@
banner.log: banner.cs
	set -euxo pipefail; \
	{ ./$< '$(BANNER)' 2>&1 1>&3 3>&- | tee $(@:.log=.err); } \
	 3>&1 1>&2 | tee $@
%.log: %.pgm ../../casperscript .FORCE
	[ -f $@ ] && mv -f $@ $*.$(TIMESTAMP).log
	cs -- ../../casperscript/lib/viewpbm.ps $+ | tee $@
%.gv: %.cs
	/usr/bin/gs -I $(PWD) <(tail -n +2 $<)
%.log: %.cs $(TXTS) .FORCE
	# https://stackoverflow.com/a/9113604/493161 fixes stderr pipe
	# https://stackoverflow.com/a/6872163/493161 and
	# https://gist.github.com/mohanpedala/1e2ff5661761d3abd0385e8223e16425
	# fix post-tee exit status
	([ -f $@ ] && mv -f $@ $*.$(TIMESTAMP).log) || true
	([ -f $*.err ] && mv -f $*.err $*.$(TIMESTAMP).err) || true
	if [ "$*" = "latin1font" ]; then \
	 arg=TimesNewRoman; \
	elif [ "$*" = "noosepaper" ]; then \
	 arg="$(TEXTFILE) $(BANNER)"; \
	else \
	 arg=$(TEXTFILE); \
	fi; \
	set -euxo pipefail; \
	{ ./$< $$arg 2>&1 1>&3 3>&- | tee $*.err; } \
	 3>&1 1>&2 | tee $@
../../casperscript:
	@echo Must git clone casperscript to the grandparent directory. >&2
	false
upload:
	rsync -avuz $(DRYRUN) $(DELETE) \
	 --exclude='Makefile' \
	 --exclude='README.md' \
	 --exclude='*.log' \
	 --exclude='*.err' \
	 --exclude='.gitignore' \
	 . $(SERVER):$(DOCROOT)/$(SERVER)/
%.txt: %.pdf
	pdftotext -nopgbrk -eol unix -enc Latin1 $<
edit:
	vi columns.cs paragraphs.cs
push:
	git push  # to default repository
	git push githost  # to personal repo, must be added to /etc/hosts
env:
ifneq ($(SHOWENV),)
	$@
else
	$(MAKE) SHOWENV=1 $@
endif
cs ccs:
	$@
clean:
	rm -f *.[0-9]*.log *.[0-9]*.err
distclean: clean
	rm -f *.log $(filter-out $(TEXTFILE), $(wildcard *.txt))
.PRECIOUS: %.ps %.log %.txt
.FORCE:
