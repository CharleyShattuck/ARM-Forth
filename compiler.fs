\ Generic Target Compiler.

0 [if]
Copyright (C) 2009-2015 by Charles Shattuck.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

For LGPL information:   http://www.gnu.org/copyleft/lesser.txt

[then]

only forth also definitions
vocabulary targ

nowarn
: target only forth also targ also definitions ; immediate
: ] postpone target ; immediate
: host only targ also forth also definitions ; immediate
: [ postpone host ; immediate
host
warn

: :m postpone target : ;
: m; postpone ; ; immediate

:m words  words m;

\ as far as gforth is concerned, the target image is byte addressed
\ even though the AVR memory is word addressed. Branch addresses
\ need to be word addressed, so begin = here 2/ for example
create target-image target-size allot
target-image target-size $ff fill  \ ROM erased.
: there   ( a1 - a2)   target-image + ;
: !-t  ( n a - )   there over 8 rshift over 1 + c! c! ;
\ : @-t  ( a - n)  there count swap c@ 8 lshift + ; \ ??

variable tdp  \ Rom pointer.
:m HERE   (  - a)   tdp @ m;
:m ORG   ( a - )   tdp ! m;
:m ALLOT   ( n - )   tdp +! m;
:m ,   ( n - )   HERE !-t 2 ALLOT m;
: ,-t   ( n - )   target , m;
: report  cr ." HERE=" target HERE host u. cr ;

variable trp  \ Ram pointer.
: cpuHERE  (  - a)   trp @ ;
: cpuORG  ( a - )  trp ! ; 0 cpuORG
: cpuALLOT  ( n - )  trp +! ;

\ ----- Optimization ----- /
variable 'edge
: hide target-size 1 - 'edge ! ; hide
: hint target here host 'edge ! ;
: edge 'edge @ ;

\ ----- Labels ----- /
nowarn
variable labels 0 labels !
warn
: label  (  - )
	[ labels @ here labels ! , ] HERE host , BL word count here
	over char+ allot place align ;
: show  ( a - ) 2 cells + count type ;
: label?  ( a - 0|a)
	>r labels begin @ dup while  dup cell+ @ r@ = if
	r> drop exit  then  repeat  r> drop ;
nowarn
: (words  words ;
: .words  labels begin  @ dup while  dup cell+ @ 2/
   base @ >r hex u. r> base ! dup show 2
   spaces  repeat  drop ;
: target-words
   labels begin  @ dup while  dup show space  repeat  drop ;
warn

create _crlf  2 c, 13 c, 10 c,
create _comma  3 c, 32 c, char , c, 32 c,
: (.) ( n - a n)  0 <# #s #> ;
0 value save-fid
: spit ( a n)  save-fid write-file abort" write error" ;
: crlf  _crlf count spit ;
: save  (  - )
	0 to save-fid   s" ./memory.h" delete-file drop
	s" ./memory.h" r/w create-file abort" Error creating memory.h" to save-fid
    s" // memory.h" save-fid write-file abort" write error" crlf
    crlf
    s" const uint16_t memory[] = {" spit crlf
    crlf
    target-image target-size 2/ 0 do
        dup w@ (.) spit _comma count spit 2 +
        i 7 and 0= if crlf then
    loop drop
    s" };" spit crlf
	save-fid close-file abort" Error closing memory.h" ;

\ ----- Headers on the target ----- /
variable thp
\ create target-heads target-size allot
\ create end-of-heads
\ : headsize end-of-heads thp @ - ;
\ target-heads target-size + 3 - thp !
target-image target-size + 4 - thp !
0 thp @ !
\ 0 value heads
nowarn
: header (  - )
   thp @ >r  labels @ cell+ dup cell+ dup c@ 3 + dup 1 and + negate thp +!
   thp @ over c@ 1 + dup 1 and + move  @ 2/ dup 8 rshift  r@ 1 - c!  r> 2 - c! ;
warn
0 [if]
\ Tack headers onto end of code.
: headers  (  - )
	target-size target here host headsize + - 0<
	abort" Target memory overflow"
	thp @ target here host dup to heads there headsize move
	headsize tdp +! ;
[then]

