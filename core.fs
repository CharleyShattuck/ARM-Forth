\ core.fs 

0 [if]
Copyright (C) 2016-2021 by Charles Shattuck.

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
decimal warnings off
host
: -code  
   \ make a label and restore input stream
   host >in @ label >in !
   \ make a word in the host and remember current target address
   create target here 2/ host ,  \ code memory is word addressed
      \ suppress optimizations and switch to target vocab (postponed),
      hide postpone target
   \ runtime behavior is to lay down a code field in the target
   does> @ ,-t ;
: code  -code header ;
: turnkey target here 2/ 0 !-t ;

target 2 org \ target-image is byte addressed here on the Forth side
code exit  1 ,
code abort  2 ,
code quit  3 ,
code emit  4 ,
code key  5 ,
-code lit  6 ,
-code branch  7 ,
-code 0branch  8 ,
-code +branch  9 ,
code (next)  10 ,
code >r  11 ,
code r>  12 ,
code r@  13 ,
code .sh  15 ,
code d#  16 ,
code counter  17 ,
code timer  18 ,
code dup  19 ,
code drop  20 ,
code swap  21 ,
code over  22 ,
code +  23 ,
code -  24 ,
code ms  25 ,
code cr  26 ,
code and  27 ,
code or  28 ,
code xor  29 ,
code invert  30 ,
code negate  31 ,
code abs  32 ,
code 2*  33 ,
code 2/  34 ,
code c@  35 ,
code @  36 ,
code @+  37 ,
code @p+  38 ,
code a  39 ,
code a!  40 ,
code p  41 ,
code p!  42 ,
code w!+  43 ,
code @p  44 ,
code c!  45 ,
code !  46 ,
code c!+  47 ,
code !+  48 ,
code depth  49 ,
code execute  50 ,
code huh?  51 ,
code c@+  52 ,
code w@+  53 ,
code um*  54 ,
code um/mod  55 ,
code w@  56 ,
code w!  57 ,
code dnegate  58 ,
-code (s")  59 ,
code nip  60 ,
code initMCP23017  61 ,
code @MCP23017  62 ,
code initGPIO  63 ,
code @GPIO  64 ,
code lshift  65 ,
code rshift  65 ,

:m begin (  - a)  here m;
:m again ( a)  branch [ 2/ ] , m;
\ 0branch doesn't drop the stack
:m until ( a)  0branch [ 2/ ] , m;
:m -until ( a)  +branch [ 2/ ] , m;
:m then ( a)  here [ 2/ swap ] !-t ;
:m if (  - a)  0branch begin 0 , m;
:m -if (  - a)  +branch begin 0 , m;
:m while ( a1 - a2 a1)  if [ swap ] m;
:m -while ( a1 - a2 a3)  -if [ swap ] m;
:m repeat ( a1 a2 - )  again then m;
:m for  >r begin m;
:m next  (next) [ 2/ ] , m;
:m -:  -code  0 , m;
:m :  code  0 , m;
:m ;  exit m;
:m cvariable  code 14 , cpuHERE , 1 cpuALLOT m;
:m wvariable  code 14 , cpuHERE , 2 cpuALLOT m;
:m variable  code 14 , cpuHERE , 4 cpuALLOT m;

\ think of #, as a literal instruction in an assembler
:m #,  lit [ dup $ffff and ] , [ $10000 / $ffff and ] , m;
:m s"  (s") here 0 , [ [char] " word count 0 do
        count ,-t loop drop ]
    here [ over - 2/ 1 - swap !-t ] m;
: 1+  1 #, + ;
: 1-  -1 #, + ;
: ptype ( a l - )  swap p! 1- for @p+ emit next ;
:m ."  s" ptype m;

variable tib 30 cpuALLOT
variable pad 30 cpuALLOT
: rot ( a b c - b c a)  >r swap r> swap ;
: count ( a1 - a2 c)  dup 1+ swap c@ ;
: space  32 #, emit ;
: type ( a l - )  1- for count emit next drop ;

: * ( n1 n2 - n3)  um* drop ;
\ unsigned versions of / and mod
: / ( n1 n2 - n3)  0 #, swap um/mod swap drop ;
: mod ( n1 n2 - n2)  0 #, swap um/mod drop ;
: ud/mod ( ud u - urem udquo)
    >r 0 #, r@ um/mod  r> swap >r um/mod r> ;
\ unsigned division
: */mod ( n1 n2 n3 - r q)  >r um* r> um/mod ;
: */ ( n1 n2 n3 - q)  */mod nip ;
wvariable holder
: hold ( c)  holder w@ 1- dup holder w! c! ;
: sign ( c)  -if [ char - ] #, hold then drop ;
: <#  pad holder w! ;
: #> ( ud - adr len)  drop drop holder w@ pad over - ;
cvariable base
: decimal  $0a #, base c! ;
: hex  $10 #, base c! ;
: # ( ud1 - ud2)
    base c@ ud/mod rot 9 #, over - -if drop 7 #, + dup then 
    drop 48 #, + hold ;
: #s ( ud - 0 0)
    begin # over over or while drop repeat drop ;
: ud. ( ud)  <# #s #> type space ;
: u. ( u)  0 #, ud. ;
: dabs ( d - +d)  -if dnegate then ;
: d. ( d)  dup >r dabs <# #s r> sign #> type space ;
: 0< ( n - flag)  -if drop -1 #, exit then drop 0 #, ;
: . ( n)  dup 0< d. ;

: false  0 #, ;
: true  -1 #, ;
: BL  32 #, ;
: max ( a b - c)
    over over - -if drop swap drop exit then
    drop drop ;

0 [if] \ these were used before <# # #> were defined
-: dig ( n1 - n2 n3)  dup 2/ 2/ 2/ 2/ swap
    $0f #, and $0a #, - -if $3a #, + exit then
    $61 #, + ;
: h. ( n - )
    dig >r dig >r dig >r dig >r
    dig >r dig >r dig >r
    dig emit
    r> emit r> emit r> emit r> emit
    r> emit r> emit r> emit
    drop space ;
: hw. ( n - )
    dig >r dig >r dig >r
    dig emit
    r> emit r> emit r> emit
    drop space ;
: hc. ( c - )
    dig >r dig emit r> emit drop space ;
[then]
: h. ( n - )  base c@ >r hex 0 #,
    <# # # # # # # # # #> type space r> base c! ;
: hw. ( n - )  base c@ >r hex 0 #,
    <# # # # # #> type space r> base c! ;
: hc. ( c - )  base c@ >r hex 0 #,
    <# # # #> type space r> base c! ;
\ dump memory, program and ram, in hex
: d ( a - a')  dup hw. p! space 7 #, for @p+ hw. next p ;
: r ( a - a')  dup hw. a! space 15 #, for c@+ hc. next a ;
-: .word  pad a!
    p @p $ff #, and 2/ for @p+ w!+ next
    pad count type space @p+ hw. ;

\ interpretive debugging
here [ 4 + constant dict ]
: dictionary  $a5 #, p! ;
: words  cr dictionary begin p @p while drop .word cr repeat drop ;
: tib! ( c)
    tib dup c@ 1+ over c! dup c@ + c! ;
: echo ( c - c)  dup emit ;
: query
    false tib ! false
    begin drop key BL max BL xor until BL xor echo tib!
    begin key BL max BL xor while BL xor echo tib! repeat
    drop BL tib dup c@ + 1+ c! ;
: match (  - 0|n)  \ P has been loaded
    tib a! false
    p @p $ff #, and 2/ for w@+ @p+ - or next
    @p+ swap if drop drop false exit then drop ; 
: find (  - a|0)
    dictionary  \ loads P register
    begin p @p while drop
        match if exit then drop
    repeat ;
-: digit ( n1 - n2)  $3a #, - -if 10 #, + exit then 29 #, - ; 
: h# (  - n)  0 #, \ interpret only
    begin key BL max BL xor while
        BL xor digit swap 2* 2* 2* 2* or
    repeat drop ; 
: ' (  - a)  query find ; \ interpret only
: ? @ h. ;
: 0= ( n - flag)  if drop false exit then
    drop true ;
: =  ( n1 n2 - flag)  - 0= ;
-: ?.  base c@ $10 #, - if drop . exit then drop u. ;
: .s  depth 0= if drop ." --> empty " exit then drop
    depth 1 #, = if drop dup ." --> " ?. exit then drop
    ." --> " depth dup a! begin swap >r 1- while repeat drop
    a begin r@ ?. r> swap 1- while repeat drop ;
: interpret
    begin .s cr query space find while
        execute depth -if huh? then drop
    repeat tib count type huh?

