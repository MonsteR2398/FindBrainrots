import re
import sys

path = sys.argv[1]

with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace bare TextureData.DataId with fully qualified BuildReportTool.TextureData.DataId
content = re.sub(r'(?<!BuildReportTool\.)TextureData\.DataId', 'BuildReportTool.TextureData.DataId', content)

with open(path, 'w', encoding='utf-8') as f:
    f.write(content)

print("Done")
