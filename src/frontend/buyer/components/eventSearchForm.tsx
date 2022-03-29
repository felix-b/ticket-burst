import React, { useEffect, useState } from 'react'
import { Box, Center, Flex, GridItem,Text, Button, Heading, FormControl, FormLabel, SimpleGrid, Select, RangeSlider, RangeSliderTrack, RangeSliderFilledTrack, RangeSliderThumb } from "@chakra-ui/react"

export const EventSearchForm = () => {

    const [sliderValue, setSliderValue] = useState(120)

    return (
        <Box minW='300px' maxW='300px'>
            <Center>
                <Heading size='md' mb='20px' >Discover More</Heading>
            </Center>
            <form>
                <SimpleGrid w='full' templateColumns='auto auto' columnGap='2px' rowGap='5px'>
                    <GridItem>
                        <FormLabel pt='8px'>Sports</FormLabel>
                    </GridItem>
                    <GridItem>
                        <FormControl>
                            <Select name='sports' placeholder='All Sports'>
                                <option value='key1'>Football</option>
                                <option value='key2'>Basketball</option>
                            </Select>
                        </FormControl>
                    </GridItem>
                    <GridItem>
                        <FormLabel pt='8px'>Dates</FormLabel>
                    </GridItem>
                    <GridItem>
                        <FormControl>
                            <Flex mt='5px' mb='3px' direction='row' justify='space-between' >
                                <Text>5 August</Text>
                                <Text>-</Text>
                                <Text>21 August</Text>
                            </Flex>
                            <RangeSlider defaultValue={[1, 17]} min={1} max={17} step={1}>
                                <RangeSliderTrack bg='red.100'>
                                    <RangeSliderFilledTrack bg='tomato' />
                                </RangeSliderTrack>
                                <RangeSliderThumb boxSize={5} index={0} borderColor='orange.400' />
                                <RangeSliderThumb boxSize={5} index={1} borderColor='orange.400' />
                            </RangeSlider>                                            
                        </FormControl>
                    </GridItem>
                </SimpleGrid>
                <Button width="full" mt={2} type="submit">
                    Find Events
                </Button>
            </form>                            
        </Box>
    )
}
