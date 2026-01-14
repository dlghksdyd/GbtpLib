import cantools
import json
import argparse
import os

def convert_dbc_to_mbdc2(dbc_path, output_path):
    try:
        # 확장자가 달라도 강제로 DBC로 인식
        db = cantools.database.load_file(dbc_path, database_format='dbc')
        print(f"Loaded DBC: {dbc_path}")
    except Exception as e:
        print(f"Error loading DBC file: {e}")
        return

    messages_list = []

    for msg in db.messages:
        # Cycle Time 및 Send Type 추출
        cycle_time = msg.cycle_time if msg.cycle_time is not None else 0
        
        send_type = "Unknown"
        if "GenMsgSendType" in msg.dbc.attributes:
             attr_val = msg.dbc.attributes["GenMsgSendType"]
             if hasattr(attr_val, "value"):
                 send_type = str(attr_val.value)
             else:
                 send_type = str(attr_val)
        elif cycle_time > 0:
            send_type = "Cyclic"
        else:
            send_type = "Event"

        # Message Dictionary (PascalCase)
        msg_data = {
            "Id": f"0x{msg.frame_id:X}",
            "Name": msg.Name if hasattr(msg, 'Name') else msg.name, # cantools 버전에 따라 대소문자 주의
            "Dlc": msg.length,
            "Sender": msg.senders[0] if msg.senders else "Unknown",
            "CycleTime": cycle_time,
            "SendType": send_type,
            "Signals": []
        }

        for sig in msg.signals:
            is_mux_switch = False
            mux_value = None

            if sig.is_multiplexer:
                is_mux_switch = True
            elif sig.multiplexer_ids:
                mux_value = sig.multiplexer_ids[0]
            
            byte_order = "Intel" if sig.byte_order == 'little_endian' else "Motorola"

            # Signal Dictionary (PascalCase)
            sig_data = {
                "Name": sig.name,
                "StartBit": sig.start,
                "Length": sig.length,
                "ByteOrder": byte_order,
                "IsMuxSwitch": is_mux_switch,
                "MuxValue": mux_value,
                "Factor": sig.scale,
                "Offset": sig.offset,
                "Min": sig.minimum,
                "Max": sig.maximum,
                "Unit": sig.unit if sig.unit else ""
            }
            msg_data["Signals"].append(sig_data)

        messages_list.append(msg_data)

    output_data = {
        "Description": "Converted to Custom MBDC2 Format",
        "MessageCount": len(messages_list),
        "Messages": messages_list
    }

    try:
        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(output_data, f, indent=2, ensure_ascii=False)
        print(f"Successfully created: {output_path}")
    except Exception as e:
        print(f"Error saving file: {e}")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='Convert DBC to .mbdc2 (JSON PascalCase)')
    parser.add_argument('input_dbc', help='Input DBC file path')
    parser.add_argument('output_file', help='Output .mbdc2 file path')
    
    args = parser.parse_args()
    convert_dbc_to_mbdc2(args.input_dbc, args.output_file)