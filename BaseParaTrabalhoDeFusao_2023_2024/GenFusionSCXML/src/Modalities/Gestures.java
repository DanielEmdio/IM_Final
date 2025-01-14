/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package Modalities;

import scxmlgen.interfaces.IModality;


public enum Gestures implements IModality{

    // ------------------------Complementary------------------------
    BOLD("[GESTURES][SCROLLR]", 5000),


    //----------------------------Single----------------------------
    WS_PREVIOUS("[GESTURES][PREVIOUSWS]", 5000),


    // ----------------Reduntante----------------
    APAGAR("[GESTURES][CUT]", 5000),

    // COLAR("[GESTURES][PASTE]", 5000),


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
