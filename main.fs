\ main.fs
target
variable data 4 ramALLOT
: /data  data a! 5 #, for 0 #, c!+ next ;
: under+ ( n1 n2 n3 - m1+n3 n2)  rot + swap ;
: #bits ( n1 - n2)  0 #, swap
    31 #, for -if 1 #, under+ then 2* next drop ;
: @pins (  - n)  @MCP23017 @GPIO 16 #, lshift or ;
: press (  - n)  dup begin drop @pins until ;
: release ( n1 - n2)  begin @pins while or repeat drop ;
: scan (  - n)
    begin press 30 #, ms @pins if or release exit then drop again

: mark ( mask a)  data + dup >r c@ or r> c! ;
: Gemini ( n)  /data $80 #, data c!
    dup $0100000 #, and if $40 #, 1 #, mark then drop \ S1
    dup $0200000 #, and if $10 #, 1 #, mark then drop \ T
    dup $0400000 #, and if $04 #, 1 #, mark then drop \ P
    dup $0800000 #, and if $01 #, 1 #, mark then drop \ H
    dup $1000000 #, and if $08 #, 2 #, mark then drop \ *
    dup $0008000 #, and if $02 #, 3 #, mark then drop \ F
    dup $0004000 #, and if $40 #, 4 #, mark then drop \ P
    dup $0002000 #, and if $10 #, 4 #, mark then drop \ L
    dup $0001000 #, and if $04 #, 4 #, mark then drop \ T
    dup $0000100 #, and if $01 #, 4 #, mark then drop \ D
    dup $0080000 #, and if $20 #, 1 #, mark then drop \ S2
    dup $0040000 #, and if $08 #, 1 #, mark then drop \ K
    dup $0020000 #, and if $02 #, 1 #, mark then drop \ W
    dup $0010000 #, and if $40 #, 2 #, mark then drop \ R
    dup $0000200 #, and if $04 #, 2 #, mark then drop \ *
    dup $0000001 #, and if $01 #, 3 #, mark then drop \ R
    dup $0000002 #, and if $20 #, 4 #, mark then drop \ B
    dup $0000004 #, and if $08 #, 4 #, mark then drop \ G
    dup $0000800 #, and if $02 #, 4 #, mark then drop \ S
    dup $0000400 #, and if $01 #, 5 #, mark then drop \ Z
    dup $0000008 #, and if $20 #, 2 #, mark then drop \ A
    dup $0000010 #, and if $10 #, 2 #, mark then drop \ O 
    dup $0000020 #, and if $40 #, 5 #, mark then drop \ #
    dup $0000040 #, and if $08 #, 3 #, mark then drop \ E
    dup $0000080 #, and if $04 #, 3 #, mark then drop \ U
    drop ;
variable 'spit
: spit  'spit @ execute ;
: >emit  ['] emit 'spit ! ;
: >hc.  ['] hc. 'spit ! ;
: send  data a! 5 #, for c@+ spit next ;
: emitHID ( c)
    ( Keyboard.begin) dup Keyboard.press 2 #, ms
    Keyboard.release ( Keyboard.end) ;
: navigate  $86 #, Keyboard.press $b3 a #, emitHID
    begin scan $20 #, = while/ $b3 #, emitHID repeat
    Keyboard.releaseAll ;
: go-Gemini ( n - n)
    begin
        begin scan $1000220 #, - while $1000220 #, + Gemini send repeat
        drop navigate
    again

\ NKRO keyboard mode

cvariable former
: spew ( c - )
    dup Keyboard.press
    former c@ if dup Keyboard.release then
    drop former c! ;
;
: send-NKRO ( n - )
    false former c!
    dup  $100000 #, and if/ [ char q ] #, spew then
    dup  $200000 #, and if/ [ char w ] #, spew then
    dup  $400000 #, and if/ [ char e ] #, spew then
    dup  $800000 #, and if/ [ char r ] #, spew then
    dup $1000000 #, and if/ [ char t ] #, spew then
\
    dup $8000 #, and if/ [ char u ] #, spew then
    dup $4000 #, and if/ [ char i ] #, spew then
    dup $2000 #, and if/ [ char o ] #, spew then
    dup $1000 #, and if/ [ char p ] #, spew then
    dup  $100 #, and if/ [ char [ ] #, spew then
\
    dup $80000 #, and if/ [ char a ] #, spew then
    dup $40000 #, and if/ [ char s ] #, spew then
    dup $20000 #, and if/ [ char d ] #, spew then
    dup $10000 #, and if/ [ char f ] #, spew then
    dup   $200 #, and if/ [ char g ] #, spew then
\
    dup  $01 #, and if/ [ char j ] #, spew then
    dup  $02 #, and if/ [ char k ] #, spew then
    dup  $04 #, and if/ [ char l ] #, spew then
    dup $800 #, and if/ [ char ; ] #, spew then
    dup $400 #, and if/ [ char ' ] #, spew then
\
    dup $08 #, and if/ [ char c ] #, spew then
    dup $10 #, and if/ [ char v ] #, spew then
    dup $20 #, and if/ [ char 3 ] #, spew then
    dup $40 #, and if/ [ char n ] #, spew then
    dup $80 #, and if/ [ char m ] #, spew then
    drop Keyboard.releaseAll ;
: go-NKRO
    begin scan send-NKRO again

: init  initMCP23017 initGPIO ;
turnkey decimal init Keyboard.begin
\   >hc. interpret
\   >emit go-Gemini
    go-NKRO

