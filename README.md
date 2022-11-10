[GIFje](https://github.com/TeoPara/cmrtstest/blob/master/edit%20BO(1).gif?raw=true)

# Technisch


**ECS**  
ECS is de code structuur die ik heb gebruikt hiervoor. ECS staat voor "Entity-Component-System". In deze principe is het de bedoeling om code en data te verdelen. In plaats van een script op elke object van elke enemy te plaatsen, heb ik EEN isntantie van een Enemy script, genaamd EnemySystem. In deze script, bestuur ik de behaviour van alle enemies tegelijkertijd, en gebruik in een Enemy class die alleen de data bevat die ik nodig heb. (Bijv. "Health", "CurrentFacingDirection")

**Pathfinding**  
Voor de pathfinding van de units in enemy's in deze spel, heb ik een eigen pathfinding systeem gemaakt. Het gaat als volgt: Er is een lijst van al bezochte tiles, en er is een lijst van de huidige path. Elke stap, zijn er 4 mogelijke richtingen waar de huidige positie in kan gaan, de richtingen die naar het einddoel wijzen, worden als eerst gekozen. Als een richting al bezocht was of er een obstakel is, wordt die geignoreerd. En als er geen richting meer over is, dan wordt de huidige positie teruggebracht naar vorige posities in de path lijst totdat er wel nog opties zijn.
Zo wordt er een path ge-genereerd, het is niet ge-garanteerd de kortst mogelijke path, maar het is wel een snelle manier om het te doen. Dat vind in belangrijker, aangezieen er wel tientallen verschillende characters die dit gebruiken tegelijkertijd in het spel zijn.

# Gameplay

**Doel van het spel:**  
Je moet een LZ kiezen, en dan moet je units spawnen en de units besturen om alle XALIENS te doodschieten. Je moet voorkomen dat de XALIENS je units doodmaken.... wanneer je ze allemaal hebt vermoord dan win je, maar als je units doodgaan verlies je...

**Mechanics:**  
Aan het begin van de spel kies je een LZ (landing zone) op de map zijn er 2 die plekken die je kan kiezen, ze zijn links onder en rechts onder.
Als je op I drukt, dan spawn je een unit op je muis. Je kan alleen spawnen binenn je LZ.
Rechts boven zijn de knoppen voor de orders. Wanneer je er een selecteerd, kan je RMB ingedrukt houden om units te selecteren, dan linkerclick om te interacten. Met hold, gaan al je units naar die plek toe pathfinden, en met move, kan je ingedrukt houden om je eigen path te beslissen.
De aliens gaat rondlopen en pathfinden, en als ze een van je units zien, gaan ze aanvallen.
Je units schieten automatisch, maar ze gaan niet schieten als er een comrade voor hun staat...
...dus de formatie van je groep maakt wel uit.

**Notitie:**  
In plaats van dat enemies naar jou base toe komen, heb ik beslist om juist jou naar de enemies toe te laten gaan. Ik denk dat dit gameplay leuker maakt, want anders zou je niks doen behalve stil staan met je units. Dus de Win condition is dat je al de aliens doodmaakt, en de Lose condition is dat je geen units meer hebt om te deployen.
