test:
	dotnet publish && \
	sudo cp -r build/publish/addons/swiftlys2/plugins/* /home/cs2server/serverfiles/game/csgo/addons/swiftlys2/plugins && \
	sudo chown -R cs2server:cs2server /home/cs2server/serverfiles/game/csgo/addons/swiftlys2