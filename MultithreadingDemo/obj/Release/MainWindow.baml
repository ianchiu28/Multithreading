   M S B A M L   `   `   `  ���� M  IMultithreadingDemo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:MultithreadingDemoMultithreadingDemo  #    MultithreadingDemo.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     2local clr-namespace:MultithreadingDemo     -     � Title$  
MainWindow��$	��250��$	��800��.��� .+��� -    ��Name$ LB_SR��$��共用資源目前在 :��$
��Left=�$��	16,10,0,0q�$	��Top=�$	��105���� -   $ LB_SharedResources��$
��Free��$
��Left=�$��26��$��
110,10,0,0q�$	��Top=�$��64���� -   $ LB_TA��$��Thread A��$
��Left=�$��24��$��
110,52,0,0q�$	��Top=�$��70���� -   $ LB_TB��$��Thread B��$
��Left=�$��24��$��
360,52,0,0q�$	��Top=�$��70���� -   $ LB_TC��$��Thread C��$
��Left=�$��24��$��
610,52,0,0q�$	��Top=�$��70��� -    �Name$ PB_TA��$
��Left=�$��24��$��
30,120,0,0q�$	��Top=�$	��228�� J� RenderTransformOrigin$ 0.5,0.5&�� -   $ PB_TB��$
��Left=�$��24��$��280,120,0,0q�$	��Top=�$	��228��� -	   $ PB_TC��$
��Left=�$��24��$��530,120,0,0q�$	��Top=�$	��228���� -
    ��Name$ Btn_Restart��$��Restart��$
��Left=�$��
690,15,0,0q�$	��Top=�$��75���� -   $ 
LB_20_50_A��$;��5  連接  ↑ 共享資源 ↑     自己內部運算��$
��Left=�$��
30,150,0,0q�$	��Top=�$	��228��$��28���� -   $ 
LB_20_50_B��$;��5  連接  ↑ 共享資源 ↑     自己內部運算��$
��Left=�$��280,150,0,0q�$	��Top=�$	��228��$��28���� -   $ 
LB_20_50_C��$;��5  連接  ↑ 共享資源 ↑     自己內部運算��$
��Left=�$��530,150,0,0q�$	��Top=�$	��228��$��28��A� ���	#FFF10505$
��Left=�$��25��$��
75,120,0,0q�
���   �$	��Top=�$��5��A� ���	#FFF10505$
��Left=�$��25��$��140,120,0,0q�
���   �$	��Top=�$��5��A� ���	#FFF10505$
��Left=�$��25��$��325,120,0,0q�
���   �$	��Top=�$��5��A� ���	#FFF10505$
��Left=�$��25��$��390,120,0,0q�
���   �$	��Top=�$��5��A� ���	#FFF10505$
��Left=�$��25��$��575,120,0,0q�
���   �$	��Top=�$��5��A� ���	#FFF10505$
��Left=�$��25��$��640,120,0,0q�
���   �$	��Top=�$��5���� -   $ btn_Disconnect_A��$��	已斷線��$
��Left=�$��
105,81,0,0q�$	��Top=�$��75��{�.  �� -   $ btn_Disconnect_B��$��	已斷線��$
��Left=�$��
355,81,0,0q�$	��Top=�$��75��{�.  �� -   $ btn_Disconnect_C��$��	已斷線��$
��Left=�$	��Top=�$��75��$��
605,81,0,0q�{�.  