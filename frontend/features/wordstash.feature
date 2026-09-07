# language: sv

Egenskap: WordStash
  Som besökare vill jag kunna slå upp slangord
  utan att behöva ett konto.

  Scenario: Besökare kan nå WordStash utan att vara inloggad
    Givet att jag inte är inloggad
    När jag går till "/wordstash"
    Så ska jag se sökfältet för slangord
    Och jag ska se ordet "cooked" i listan

  Scenario: Besökare söker fram ett ord och läser detaljerna
    Givet att jag inte är inloggad
    Och att jag är på WordStash
    När jag söker efter "cooked"
    Så ska listan bara innehålla ordet "cooked"
    När jag väljer ordet "cooked" i listan
    Så ska jag se detaljerna för "cooked"
    Och detaljerna ska innehålla minst ett exempel