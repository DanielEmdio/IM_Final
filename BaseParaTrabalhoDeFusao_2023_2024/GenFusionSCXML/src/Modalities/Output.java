package Modalities;

import scxmlgen.interfaces.IOutput;

public enum Output implements IOutput{
    

    /* 
    CHANGE_COLOR_TRIANGULO_AZUL("[FUSION][CHANGE_COLOR][TRIANGULO][AZUL]"),
    CHANGE_COLOR_TRIANGULO_VERDE("[FUSION][CHANGE_COLOR][TRIANGULO][VERDE]"),
    CHANGE_COLOR_TRIANGULO_CINZENTO("[FUSION][CHANGE_COLOR][TRIANGULO][CINZENTO]"),
    CHANGE_COLOR_TRIANGULO_VERMELHO("[FUSION][CHANGE_COLOR][TRIANGULO][VERMELHO]"),
    CHANGE_COLOR_TRIANGULO_BRANCO("[FUSION][CHANGE_COLOR][TRIANGULO][BRANCO]"),
    CHANGE_COLOR_TRIANGULO_ROSA("[FUSION][CHANGE_COLOR][TRIANGULO][ROSA]"),
    CHANGE_COLOR_TRIANGULO_AMARELO("[FUSION][CHANGE_COLOR][TRIANGULO][AMARELO]"),
    CHANGE_COLOR_TRIANGULO_PRETO("[FUSION][CHANGE_COLOR][TRIANGULO][PRETO]"),
    CHANGE_COLOR_TRIANGULO_LARANJA("[FUSION][CHANGE_COLOR][TRIANGULO][LARANJA]"),

    CHANGE_COLOR_QUADRADO_AZUL("[FUSION][CHANGE_COLOR][QUADRADO][AZUL]"),
    CHANGE_COLOR_QUADRADO_VERDE("[FUSION][CHANGE_COLOR][QUADRADO][VERDE]"),
    CHANGE_COLOR_QUADRADO_CINZENTO("[FUSION][CHANGE_COLOR][QUADRADO][CINZENTO]"),
    CHANGE_COLOR_QUADRADO_VERMELHO("[FUSION][CHANGE_COLOR][QUADRADO][VERMELHO]"),
    CHANGE_COLOR_QUADRADO_BRANCO("[FUSION][CHANGE_COLOR][QUADRADO][BRANCO]"),
    CHANGE_COLOR_QUADRADO_ROSA("[FUSION][CHANGE_COLOR][QUADRADO][ROSA]"),
    CHANGE_COLOR_QUADRADO_AMARELO("[FUSION][CHANGE_COLOR][QUADRADO][AMARELO]"),
    CHANGE_COLOR_QUADRADO_PRETO("[FUSION][CHANGE_COLOR][QUADRADO][PRETO]"),
    CHANGE_COLOR_QUADRADO_LARANJA("[FUSION][CHANGE_COLOR][QUADRADO][LARANJA]"),

    CHANGE_COLOR_CIRCULO_AZUL("[FUSION][CHANGE_COLOR][CIRCULO][AZUL]"),
    CHANGE_COLOR_CIRCULO_VERDE("[FUSION][CHANGE_COLOR][CIRCULO][VERDE]"),
    CHANGE_COLOR_CIRCULO_CINZENTO("[FUSION][CHANGE_COLOR][CIRCULO][CINZENTO]"),
    CHANGE_COLOR_CIRCULO_VERMELHO("[FUSION][CHANGE_COLOR][CIRCULO][VERMELHO]"),
    CHANGE_COLOR_CIRCULO_BRANCO("[FUSION][CHANGE_COLOR][CIRCULO][BRANCO]"),
    CHANGE_COLOR_CIRCULO_ROSA("[FUSION][CHANGE_COLOR][CIRCULO][ROSA]"),
    CHANGE_COLOR_CIRCULO_AMARELO("[FUSION][CHANGE_COLOR][CIRCULO][AMARELO]"),
    CHANGE_COLOR_CIRCULO_PRETO("[FUSION][CHANGE_COLOR][CIRCULO][PRETO]"),
    CHANGE_COLOR_CIRCULO_LARANJA("[FUSION][CHANGE_COLOR][CIRCULO][LARANJA]"),
    */

    //--------------Complementary------------------------
    BOLD_ESCREVER_CONTEUDO("[FUSION][BOLD_ESCREVER_CONTEUDO]"),
    ITALICO_ESCREVER_CONTEUDO("[FUSION][ITALICO_ESCREVER_CONTEUDO]"),
    SUBLINHADO_ESCREVER_CONTEUDO("[FUSION][SUBLINHADO_ESCREVER_CONTEUDO]"),

    COPIAR_SELECIONAR_CELULAS("[FUSION][COPIAR_SELECIONAR_CELULAS]"),
    CORTE_SELECIONAR_CELULAS("[FUSION][CORTE_SELECIONAR_CELULAS]"),
    COLAR_SELECIONAR_CELULAS("[FUSION][COLAR_SELECIONAR_CELULAS]"),
  

    // COPIAR_SELECIONAR_AREA("[FUSION][COPIAR_SELECIONAR_AREA]"),
    // CORTE_SELECIONAR_AREA("[FUSION][CORTE_SELECIONAR_AREA]"),
    // COLAR_SELECIONAR_AREA("[FUSION][COLAR_SELECIONAR_AREA]"),
    // APAGAR_SELECIONAR_AREA("[FUSION][APAGAR_SELECIONAR_AREA]"),

    LOCK_SELECIONAR_CELULAS("[FUSION][LOCK_SELECIONAR_CELULAS]"),
    LOCK_PROCURAR_COLUNA("[FUSION][LOCK_PROCURAR_COLUNA]"),
    LOCK_PROCURAR_LINHA("[FUSION][LOCK_PROCURAR_LINHA]"),
    LOCK_PROCURAR("[FUSION][LOCK_PROCURAR]"),

    //--------------Single----------------------------------
    SELECIONAR_AREA("[FUSION][SELECIONAR_AREA]"),
	SELECIONAR_CELULAS("[FUSION][SELECIONAR_CELULAS]"),
	ESCREVER_CONTEUDO("[FUSION][ESCREVER_CONTEUDO]"),
	ALTERAR_TAMANHO_TEXTO("[FUSION][ALTERAR_TAMANHO_TEXTO]"),
	DIMINUIR_TAMANHO_TEXTO("[FUSION][DIMINUIR_TAMANHO_TEXTO]"),
	ESTILO_TEXTO("[FUSION][ESTILO_TEXTO]"),
	CHANGE_COLOR("[FUSION][CHANGE_COLOR]"),
	DEFINIR_LIMITES("[FUSION][DEFINIR_LIMITES]"),
	SALVAR("[FUSION][SALVAR]"),
	FECHAR("[FUSION][FECHAR]"),
	AJUDA("[FUSION][AJUDA]"),
	LIMPAR("[FUSION][LIMPAR]"),
	DIRECIONAR("[FUSION][DIRECIONAR]"),
	ORIENTAR("[FUSION][ORIENTAR]"),
	MATEMATICA("[FUSION][MATEMATICA]"),

    PROCURAR("[FUSION][PROCURAR]"),
    SELECIONAR_COLUNA("[FUSION][SELECIONAR_COLUNA]"),
    SELECIONAR_LINHA("[FUSION][SELECIONAR_LINHA]"),

    GREET("[FUSION][GREET]"),
    GOODBYE("[FUSION][GOODBYE]"),
    AFFIRM("[FUSION][AFFIRM]"),
    DENY("[FUSION][DENY]"),

    SELECIONAR_X_AREA("[FUSION][SELECIONAR_X_AREA]"),
    PROCURAR_COLUNA("[FUSION][PROCURAR_COLUNA]"),
    PROCURAR_LINHA("[FUSION][PROCURAR_LINHA]"),

    //gestures
    WS_PREVIOUS("[FUSION][PREVIOUSWS]"),
    WS_NEXT("[FUSION][NEXTWS]"),
    ZOOMIN("[FUSION][ZOOMIN]"),
    ZOOMOUT("[FUSION][ZOOMOUT]"),

    SCROLLUP("[FUSION][SCROLLUP]"),
    SCROLLDOWN("[FUSION][SCROLLDOWN]"),
    SCROLLLEFT("[FUSION][SCROLLLEFT]"),
    SCROLLRIGHT("[FUSION][SCROLLRIGHT]"),
    
    //--------------Reduntante-----------------------------
    COLAR("[FUSION][COLAR]"),
    APAGAR("[FUSION][APAGAR]"),
    CORTAR("[FUSION][CORTAR]"),
    COPIAR("[FUSION][COPIAR]"),

    ;
    
    
    
    private String event;

    Output(String m) {
        event=m;
    }
    
    public String getEvent(){
        return this.toString();
    }

    public String getEventName(){
        return event;
    }
}
