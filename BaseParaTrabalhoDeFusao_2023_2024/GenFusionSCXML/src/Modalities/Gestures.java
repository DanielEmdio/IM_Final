/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package Modalities;

import scxmlgen.interfaces.IModality;


public enum Gestures implements IModality{

    // ------------------------Complementary------------------------
    NEGRITO("[GESTURES][BOLD]", 4000), //por implmentar
    ITALICO("[GESTURES][ITALIC]",4000), //por implmentar
    SUBLINHADO("[GESTURES][UNDERLINE]",4000), //por implmentar

    CORTE("[GESTURES][CUT]",1500), 
    APAGAR("[GESTURES][DELETE]", 4000), 
    COPIAR("[GESTURES][COPIAR]",4000), //por implmentar
    COLAR("[GESTURES][PASTE]", 4000), 

    LOCKIN("[GESTURES][LOCKI]", 4000), //por implementar

    // WS_PREVIOUS("[GESTURES][PREVIOUSWS]",5000), 
    // WS_NEXT("[GESTURES][NEXTWS]",5000),

    //----------------------------Single----------------------------
    

    ZOOMIN("[GESTURES][ZOOMIN]",5000),
    ZOOMOUT("[GESTURES][ZOOMOUT]",5000),
    SCROLLUP("[GESTURES][SCROLLU]", 1500), //por implementar
    SCROLLDOWN("[GESTURES][SCROLLD]", 1500), //por implementar
    SCROLLLEFT("[GESTURES][SCROLLL]", 1500), //por implementar
    SCROLLRIGHT("[GESTURES][SCROLLR]", 1500), //por implementar

    ;
    
    private String event;
    private int timeout;


    Gestures(String m, int time) {
        event=m;
        timeout=time;
    }

    @Override
    public int getTimeOut() {
        return timeout;
    }

    @Override
    public String getEventName() {
        //return getModalityName()+"."+event;
        return event;
    }

    @Override
    public String getEvName() {
        return getModalityName().toLowerCase()+event.toLowerCase();
    }
    
}
