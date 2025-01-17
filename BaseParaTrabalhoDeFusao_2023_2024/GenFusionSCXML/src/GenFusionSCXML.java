/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */

import java.io.IOException;
import scxmlgen.Fusion.FusionGenerator;
//import FusionGenerator;

import Modalities.Output;
import Modalities.Speech;
import Modalities.Touch;
import Modalities.Gestures;

/**
 *
 * @author nunof
 */
public class GenFusionSCXML {

    /**
     * @param args the command line arguments
     */
  public static void main(String[] args) throws IOException {

    FusionGenerator fg = new FusionGenerator();
    
    /*  
    fg.Complementary(Speech.CHANGE_COLOR_AZUL, Touch.SHAPE_TRIANGULO, Output.CHANGE_COLOR_TRIANGULO_AZUL);
    fg.Complementary(Speech.CHANGE_COLOR_VERDE, Touch.SHAPE_TRIANGULO, Output.CHANGE_COLOR_TRIANGULO_VERDE);
    fg.Complementary(Speech.CHANGE_COLOR_CINZENTO, Touch.SHAPE_TRIANGULO, Output.CHANGE_COLOR_TRIANGULO_CINZENTO);
    fg.Complementary(Speech.CHANGE_COLOR_VERMELHO, Touch.SHAPE_TRIANGULO, Output.CHANGE_COLOR_TRIANGULO_VERMELHO);
    fg.Complementary(Speech.CHANGE_COLOR_BRANCO, Touch.SHAPE_TRIANGULO, Output.CHANGE_COLOR_TRIANGULO_BRANCO);
    fg.Complementary(Speech.CHANGE_COLOR_ROSA, Touch.SHAPE_TRIANGULO, Output.CHANGE_COLOR_TRIANGULO_ROSA);
    fg.Complementary(Speech.CHANGE_COLOR_AMARELO, Touch.SHAPE_TRIANGULO, Output.CHANGE_COLOR_TRIANGULO_AMARELO);
    fg.Complementary(Speech.CHANGE_COLOR_PRETO, Touch.SHAPE_TRIANGULO, Output.CHANGE_COLOR_TRIANGULO_PRETO);
    fg.Complementary(Speech.CHANGE_COLOR_LARANJA, Touch.SHAPE_TRIANGULO, Output.CHANGE_COLOR_TRIANGULO_LARANJA);

    fg.Complementary(Speech.CHANGE_COLOR_AZUL, Touch.SHAPE_QUADRADO, Output.CHANGE_COLOR_QUADRADO_AZUL);
    fg.Complementary(Speech.CHANGE_COLOR_VERDE, Touch.SHAPE_QUADRADO, Output.CHANGE_COLOR_QUADRADO_VERDE);
    fg.Complementary(Speech.CHANGE_COLOR_CINZENTO, Touch.SHAPE_QUADRADO, Output.CHANGE_COLOR_QUADRADO_CINZENTO);
    fg.Complementary(Speech.CHANGE_COLOR_VERMELHO, Touch.SHAPE_QUADRADO, Output.CHANGE_COLOR_QUADRADO_VERMELHO);
    fg.Complementary(Speech.CHANGE_COLOR_BRANCO, Touch.SHAPE_QUADRADO, Output.CHANGE_COLOR_QUADRADO_BRANCO);
    fg.Complementary(Speech.CHANGE_COLOR_ROSA, Touch.SHAPE_QUADRADO, Output.CHANGE_COLOR_QUADRADO_ROSA);
    fg.Complementary(Speech.CHANGE_COLOR_AMARELO, Touch.SHAPE_QUADRADO, Output.CHANGE_COLOR_QUADRADO_AMARELO);
    fg.Complementary(Speech.CHANGE_COLOR_PRETO, Touch.SHAPE_QUADRADO, Output.CHANGE_COLOR_QUADRADO_PRETO);
    fg.Complementary(Speech.CHANGE_COLOR_LARANJA, Touch.SHAPE_QUADRADO, Output.CHANGE_COLOR_QUADRADO_LARANJA);

    fg.Complementary(Speech.CHANGE_COLOR_AZUL, Touch.SHAPE_CIRCULO, Output.CHANGE_COLOR_CIRCULO_AZUL);
    fg.Complementary(Speech.CHANGE_COLOR_VERDE, Touch.SHAPE_CIRCULO, Output.CHANGE_COLOR_CIRCULO_VERDE);
    fg.Complementary(Speech.CHANGE_COLOR_CINZENTO, Touch.SHAPE_CIRCULO, Output.CHANGE_COLOR_CIRCULO_CINZENTO);
    fg.Complementary(Speech.CHANGE_COLOR_VERMELHO, Touch.SHAPE_CIRCULO, Output.CHANGE_COLOR_CIRCULO_VERMELHO);
    fg.Complementary(Speech.CHANGE_COLOR_BRANCO, Touch.SHAPE_CIRCULO, Output.CHANGE_COLOR_CIRCULO_BRANCO);
    fg.Complementary(Speech.CHANGE_COLOR_ROSA, Touch.SHAPE_CIRCULO, Output.CHANGE_COLOR_CIRCULO_ROSA);
    fg.Complementary(Speech.CHANGE_COLOR_AMARELO, Touch.SHAPE_CIRCULO, Output.CHANGE_COLOR_CIRCULO_AMARELO);
    fg.Complementary(Speech.CHANGE_COLOR_PRETO, Touch.SHAPE_CIRCULO, Output.CHANGE_COLOR_CIRCULO_PRETO);
    fg.Complementary(Speech.CHANGE_COLOR_LARANJA, Touch.SHAPE_CIRCULO, Output.CHANGE_COLOR_CIRCULO_LARANJA);

    
    fg.Sequence(Speech.SQUARE, SecondMod.RED, Output.SQUARE_RED);
    fg.Sequence(Speech.SQUARE, SecondMod.BLUE, Output.SQUARE_BLUE);
    fg.Sequence(Speech.SQUARE, SecondMod.YELLOW, Output.SQUARE_YELLOW);
    fg.Sequence(Speech.TRIANGLE, SecondMod.RED, Output.TRIANGLE_RED);
    fg.Sequence(Speech.TRIANGLE, SecondMod.BLUE, Output.TRIANGLE_BLUE);
    fg.Sequence(Speech.TRIANGLE, SecondMod.YELLOW, Output.TRIANGLE_YELLOW);
    fg.Redundancy(Speech.CIRCLE, SecondMod.RED, Output.CIRCLE_RED);
    fg.Redundancy(Speech.CIRCLE, SecondMod.BLUE, Output.CIRCLE_BLUE);
    fg.Redundancy(Speech.CIRCLE, SecondMod.YELLOW, Output.CIRCLE_YELLOW);
    
    fg.Single(Speech.CIRCLE, Output.CIRCLE);
    
    
    fg.Redundancy(Speech.OPEN_SOCIAL, SecondMod.RED, Output.OPEN_SOCIAL);
    fg.Single(Speech.OPEN_SOCIAL, Output.OPEN_SOCIAL);
  
  
    fg.Redundancy(Speech.OPEN_SOCIAL, SecondMod.SOCIAL, Output.OPEN_SOCIAL);
  
    fg.Redundancy(Speech.OPEN_LIXO, SecondMod.LIXO, Output.OPEN_LIXO);
    fg.Single(Speech.OPEN_LIXO, Output.OPEN_LIXO);
    
    
    fg.Build("fusion.scxml");
      
  

    fg.Complementary(Speech.LIGHT_ON, Touch.LOCATION_LIVINGROOM, Output.LIGHT_LIVINGROOM_ON);
    fg.Complementary(Speech.LIGHT_ON, Touch.LOCATION_ROOM, Output.LIGHT_ROOM_ON);
    fg.Complementary(Speech.LIGHT_ON, Touch.LOCATION_KITCHEN, Output.LIGHT_KITCHEN_ON);
    fg.Complementary(Speech.LIGHT_OFF, Touch.LOCATION_LIVINGROOM, Output.LIGHT_LIVINGROOM_OFF);
    fg.Complementary(Speech.LIGHT_OFF, Touch.LOCATION_ROOM, Output.LIGHT_ROOM_OFF);
    fg.Complementary(Speech.LIGHT_OFF, Touch.LOCATION_KITCHEN, Output.LIGHT_KITCHEN_OFF);  
    
    fg.Complementary(Touch.LOCATION_LIVINGROOM, Speech.LIGHT_ON, Output.LIGHT_LIVINGROOM_ON);
    fg.Complementary(Touch.LOCATION_ROOM, Speech.LIGHT_ON, Output.LIGHT_ROOM_ON);
    fg.Complementary(Touch.LOCATION_KITCHEN, Speech.LIGHT_ON, Output.LIGHT_KITCHEN_ON);
    fg.Complementary(Touch.LOCATION_LIVINGROOM, Speech.LIGHT_OFF, Output.LIGHT_LIVINGROOM_OFF);
    fg.Complementary(Touch.LOCATION_ROOM, Speech.LIGHT_OFF, Output.LIGHT_ROOM_OFF);
    fg.Complementary(Touch.LOCATION_KITCHEN, Speech.LIGHT_OFF, Output.LIGHT_KITCHEN_OFF);  
    
    //
    fg.Complementary(Speech.TEMPERATURE_UP, Touch.LOCATION_LIVINGROOM, Output.TEMP_LIVINGROOM_UP);
    fg.Complementary(Speech.TEMPERATURE_UP, Touch.LOCATION_ROOM, Output.TEMP_ROOM_UP);
    fg.Complementary(Speech.TEMPERATURE_UP, Touch.LOCATION_KITCHEN, Output.TEMP_KITCHEN_UP);
    fg.Complementary(Speech.TEMPERATURE_DOWN, Touch.LOCATION_LIVINGROOM, Output.TEMP_LIVINGROOM_DOWN);
    fg.Complementary(Speech.TEMPERATURE_DOWN, Touch.LOCATION_ROOM, Output.TEMP_ROOM_DOWN);
    fg.Complementary(Speech.TEMPERATURE_DOWN, Touch.LOCATION_KITCHEN, Output.TEMP_KITCHEN_DOWN);  
    
    fg.Complementary(Touch.LOCATION_LIVINGROOM, Speech.TEMPERATURE_UP, Output.TEMP_LIVINGROOM_UP);
    fg.Complementary(Touch.LOCATION_ROOM, Speech.TEMPERATURE_UP, Output.TEMP_ROOM_UP);
    fg.Complementary(Touch.LOCATION_KITCHEN, Speech.TEMPERATURE_UP, Output.TEMP_KITCHEN_UP);
    fg.Complementary(Touch.LOCATION_LIVINGROOM, Speech.TEMPERATURE_DOWN, Output.TEMP_LIVINGROOM_DOWN);
    fg.Complementary(Touch.LOCATION_ROOM, Speech.TEMPERATURE_DOWN, Output.TEMP_ROOM_DOWN);
    fg.Complementary(Touch.LOCATION_KITCHEN, Speech.TEMPERATURE_DOWN, Output.TEMP_KITCHEN_DOWN); 
    

    fg.Single(Speech.LIGHT_ON, Output.LIGHT_ON);
    fg.Single(Speech.LIGHT_OFF, Output.LIGHT_OFF);
    fg.Single(Touch.LOCATION_LIVINGROOM, Output.LOCATION_LIVINGROOM);
    fg.Single(Touch.LOCATION_ROOM, Output.LOCATION_ROOM);
    fg.Single(Touch.LOCATION_KITCHEN, Output.LOCATION_KITCHEN);
    
    fg.Single(Speech.TEMPERATURE_UP, Output.TEMP_UP);
    fg.Single(Speech.TEMPERATURE_DOWN, Output.TEMP_DOWN);
    
      
      
    //fg.Complementary(Touch.OPEN_NEWS_TITLE, Speech.ACTION_NEWS_NIMAGE, Output.OPEN_NEWS_AS_IMAGE);
    // fg.Complementary(Speech.ACTION_NEWS_NTEXT,Touch.OPEN_NEWS_TITLE, Output.OPEN_NEWS_AS_TEXT);
    // fg.Complementary(Speech.ACTION_NEWS_NIMAGE,Touch.OPEN_NEWS_TITLE, Output.OPEN_NEWS_AS_IMAGE);
    // fg.Single(Touch.OPEN_NEWS_TITLE, Output.OPEN_NEWS_AS_TEXT);
      
    // fg.Redundancy(Touch.GO_BACK, Speech.ACTION_GENERICENTITY_BACK, Output.GO_BACK);
    */

    //---------------------------------------Complementary--------------------------------------------------------
    fg.Complementary(Speech.ESCREVER_CONTEUDO, Gestures.NEGRITO, Output.BOLD_ESCREVER_CONTEUDO);
    fg.Complementary(Speech.ESCREVER_CONTEUDO, Gestures.ITALICO, Output.ITALICO_ESCREVER_CONTEUDO);
    fg.Complementary(Speech.ESCREVER_CONTEUDO, Gestures.SUBLINHADO, Output.SUBLINHADO_ESCREVER_CONTEUDO); 

    fg.Complementary(Speech.SELECIONAR_CELULAS, Gestures.COPIAR, Output.COPIAR_SELECIONAR_CELULAS);
    fg.Complementary(Speech.SELECIONAR_CELULAS, Gestures.CORTE, Output.CORTE_SELECIONAR_CELULAS);
    fg.Complementary(Speech.SELECIONAR_CELULAS, Gestures.COLAR, Output.COLAR_SELECIONAR_CELULAS);
    
    fg.Complementary(Speech.FECHAR,Gestures.APAGAR, Output.FECHAR);
    fg.Complementary(Speech.APAGAR,Gestures.APAGAR, Output.APAGAR);

    fg.Complementary(Speech.SELECIONAR_CELULAS,Gestures.LOCKIN, Output.LOCK_SELECIONAR_CELULAS);
    fg.Complementary(Speech.PROCURAR_COLUNA, Gestures.LOCKIN, Output.LOCK_PROCURAR_COLUNA);
    fg.Complementary(Speech.PROCURAR_LINHA, Gestures.LOCKIN, Output.LOCK_PROCURAR_LINHA);
    fg.Complementary(Speech.PROCURAR, Gestures.LOCKIN, Output.LOCK_PROCURAR);

    fg.Complementary(Speech.WS, Gestures.SCROLLLEFT, Output.WS_PREVIOUS);
    fg.Complementary(Speech.WS, Gestures.SCROLLRIGHT, Output.WS_NEXT);


    //---------------------------------------Single---------------------------------------------------------------
    //speech
    fg.Single(Speech.SELECIONAR_AREA, Output.SELECIONAR_AREA);
    fg.Single(Speech.SELECIONAR_CELULAS, Output.SELECIONAR_CELULAS);
    fg.Single(Speech.ESCREVER_CONTEUDO, Output.ESCREVER_CONTEUDO);
    fg.Single(Speech.ALTERAR_TAMANHO_TEXTO, Output.ALTERAR_TAMANHO_TEXTO);
    fg.Single(Speech.DIMINUIR_TAMANHO_TEXTO, Output.DIMINUIR_TAMANHO_TEXTO);
    fg.Single(Speech.ESTILO_TEXTO, Output.ESTILO_TEXTO);
    fg.Single(Speech.CHANGE_COLOR, Output.CHANGE_COLOR);
    fg.Single(Speech.DEFINIR_LIMITES, Output.DEFINIR_LIMITES);
    fg.Single(Speech.SALVAR, Output.SALVAR);
    fg.Single(Speech.AJUDA, Output.AJUDA);
    fg.Single(Speech.LIMPAR, Output.LIMPAR);
    fg.Single(Speech.DIRECIONAR, Output.DIRECIONAR);
    fg.Single(Speech.ORIENTAR, Output.ORIENTAR);
    fg.Single(Speech.MATEMATICA, Output.MATEMATICA);

    //novo
    fg.Single(Speech.PROCURAR, Output.PROCURAR);
    fg.Single(Speech.SELECIONAR_COLUNA, Output.SELECIONAR_COLUNA);
    fg.Single(Speech.SELECIONAR_LINHA, Output.SELECIONAR_LINHA);

    fg.Single(Speech.GREET, Output.GREET);
    fg.Single(Speech.GOODBYE, Output.GOODBYE);
    fg.Single(Speech.AFFIRM, Output.AFFIRM);
    fg.Single(Speech.DENY, Output.DENY);
    fg.Single(Speech.ORIGEM, Output.ORIGEM);
    
    fg.Single(Speech.SELECIONAR_X_AREA, Output.SELECIONAR_X_AREA);
    fg.Single(Speech.PROCURAR_COLUNA, Output.PROCURAR_COLUNA);
    fg.Single(Speech.PROCURAR_LINHA, Output.PROCURAR_LINHA);



    //gesture
    fg.Single(Gestures.ZOOMIN, Output.ZOOMIN);
    fg.Single(Gestures.ZOOMOUT, Output.ZOOMOUT);
    fg.Single(Gestures.SCROLLUP, Output.SCROLLUP);
    fg.Single(Gestures.SCROLLDOWN, Output.SCROLLDOWN);
    fg.Single(Gestures.SCROLLLEFT, Output.SCROLLLEFT);
    fg.Single(Gestures.SCROLLRIGHT, Output.SCROLLRIGHT);

   
    //---------------------------------------Redundant--------------------------------------------------------
    fg.Redundancy(Speech.COLAR, Gestures.COLAR, Output.COLAR);
    fg.Redundancy(Speech.CORTAR, Gestures.CORTE, Output.CORTAR);
    fg.Redundancy(Speech.COPIAR, Gestures.COPIAR, Output.COPIAR);


    fg.Build("fusion.scxml");
        
  }
    
}
