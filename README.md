O projeto pode ser testado através da execução do projeto baixado pelo repositório ou pela execução da build linkada na [release disponibilizada no repositório](https://github.com/viniciusguerra/BejeweledTest/releases/tag/Test2).

Na documentação abaixo serão expressas as linhas de raciocínio por trás do desenvolvimento e as melhorias e implementações que não consegui fazer a tempo. Segue também, conforme playesting, um comparativo dos objetivos requeridos no documento do teste com o que foi concluído:

# Objetivos
* Tabuleiro 8x8 com 5 peças possíveis (5 cores): **CONCLUÍDO**
* O jogador pode trocar verticalmente ou horizontalmente 2 peças adjacentes: **CONCLUÍDO**
* Se a troca resultar em 3 ou mais peças do mesmo tipo em linhas ou colunas, elas
desaparecem: **CONCLUÍDO**
* Se a troca não resultar em nenhuma sequência, as peças voltam para suas posições
originais: **CONCLUÍDO**
* Quando qualquer peça desaparece, as peças acima da mesma cairão em seu lugar e
novas peças surgirão do topo do tabuleiro preenchendo o lugar das anteriores: **CONCLUÍDO**
* As peças podem trocar de lugar através de Click ou Drag: **CONCLUÍDO**

# Estrutura e Técnica do Projeto

## Diretórios

Na raíz do projeto encontramos o diretório `_Bejeweled` onde estão todos os arquivos desenvolvidos e o diretório `Animated Match 3 Gems + Hue Shift Sprites Shaders` onde se encontram os gráficos baixados na Asset Store [neste link](https://assetstore.unity.com/packages/2d/environments/animated-match-3-gems-hue-shift-sprites-shaders-62804).

No diretório `_Bejeweled` encontramos a cena de jogo `Bejeweled.unity` e os três diretórios contendo as features principais do teste, `Pooling`, `Score`, `Selection`, `Table`, `Tiles` e `UI`.

### Pooling

A técnica de Pooling foi implementada para que novas gemas não sejam instanciadas ou destruídas em tempo de execução, mas retiradas de uma lista pré-instanciada. Isso ajuda a evitar o Garbage Collector, que causaria "engasgos" na execução. Uma futura implementação instanciaria os assets de forma assíncrona para que uma tela de loading responsiva pudesse ser executada ao mesmo tempo, dando feedback e melhor UX ao jogador.

### Score

A classe ScoreController recebe os matches de Tiles e a partir disso calcula a pontuação e os combos e informa classes relevantes através de um evento.

### Table

A classe responsável por uma visão geral do quadro de gemas. Ela contém a matriz de Tiles, que é a estrutura de dados principal utilizada para comparação de peças. Esta classe é composta por:
* Manutenção da estrutura de dados principal na própria classe Table
* Construção das peças pelo componente TableBuilder
* Navegação no quadro pelo componente TableNavigator
* Comparação de gemas pelo componente TableMatcher
* Animação das peças pelo componente TableTileAnimator

### Tile

A classe Tile é composta das seguintes responsabilidades: 
* Recebe interação do jogador através do componente TileInput
* É classificada e comparada através de TileTypes
* Instanciada, inicializada e tem suas dependências injetadas através da TileFactory, aplicando inversão de controle e evitando Finds, FindComponents e buscas deste tipo
* Animada em seu idle e estado de seleção pelo TileAnimator
* Comparada em direções descritas pelo enum TileDirection
* Reunida com outros Tiles próximos na classe TileMatch, criada quando há combinação de 3 ou mais peças consecutivas, e guardando também a posição do match para indicação a pontuação adquirida

### UI

A classe ScorePanel se inscreve no ScoreController para receber a nova pontuação e atualizar a contagem de pontos totais. 
A classe ComboPanel se inscreve no ScoreController para indicar quando um combo, ou seja, a combinação consecutiva de peças, é realizada.
A classe MatchIndicator também se inscreve no ScoreController e mantém uma pilha com todos os matches feitos recentemente. Um por um, ela indica a pontuação adquirida naquele match e carrega o canvas para aquela posição, fazendo uma interpolação de escala não-linear através da avaliação de uma AnimationCurve.

## Técnicas

A classe Table é muito dependente de programação assíncrona implementada na forma de corrotinas. Desta forma é garantido o procedimento correto de lógica que deve ser aguardada.

Algumas variáveis são implementadas como privadas utilizando o prefixo _ para que não sejam alocadas em escopo de método. Outras exigem a alocação em tempo de método e não podem ser utilizadas desta forma.

Inicialmente eu utilizei um sistema de Addressables para o carregamento assíncrono dos assets que compõem as gemas, como sprites e etc., mas o AnimatorController só é identificado em tempo de Editor e não de Player, impossibilitando sua troca no componente Animator e assim, as animações que são atribuídas a um objeto. O uso de Addressables evitaria a necessidade de uma Pool de objetos permitindo com que os 8x8 tiles fossem gerados e tivessem seus componentes reciclados, apenas mudando os valores como sprites e animações. Portanto, por conta das minhas restrições de tempo, optei por usar a Pool e mudar o GameObject contendo todo o pacote gráfico, o que gera um pouco mais de uso de memória, mas foi o que consegui no tempo que utilizei.

# Conclusão

Todos os requisitos na seção de Implementação do documento de teste foram completos. A implementação de uma UI dando feedback da pontuação aquirida e combos aprimora um pouco a usabilidade do game.

Na parte técnica testes unitários ajudariam a provar o funcionamento correto e tratamento de corner cases em conjunto com playtests. Na parte de UX, efeitos sonoros e de partículas enriqueceriam a experiência estética.

Agradeço a oportunidade de desenvolver o teste!
